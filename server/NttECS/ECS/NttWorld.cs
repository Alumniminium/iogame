using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using server.Helpers;

namespace server.ECS;

/// <summary>
/// Central world manager for the Entity Component System, handling entity lifecycle, systems coordination, and game loop timing.
/// Manages entity creation, destruction, system updates, and maintains consistent tick-based simulation at target framerate.
/// </summary>
public static class NttWorld
{
    /// <summary>Target ticks per second for consistent simulation timing</summary>
    public static int TargetTps { get; private set; } = 60;
    /// <summary>Time duration for each update tick in seconds</summary>
    private static float UpdateTime => 1f / TargetTps;
    /// <summary>Delta time between ticks in seconds </summary>
    // public static float DeltaTime => 1f / TargetTps;
    /// <summary>Current number of active entities in the world</summary>
    public static int EntityCount => NTTs.Count;

    private static readonly NTT[] Default = new NTT[1];
    public static readonly Dictionary<Guid, NTT> NTTs = [];
    public static readonly HashSet<NTT> Players = [];

    private static readonly ConcurrentQueue<NTT> ToBeRemoved = new();
    public static readonly ConcurrentQueue<NTT> ChangedThisTick = new();

    public static NttSystem[] Systems = Array.Empty<NttSystem>();
    public static long Tick { get; private set; }
    private static long TickBeginTime;
    private static float TimeAcc;
    private static float UpdateTimeAcc;

    private static Action OnSecond;
    private static Action OnEndTick;
    private static Action OnBeginTick;

    static NttWorld()
    {
        Default[0] = new(Guid.Empty);
        var start = Stopwatch.GetTimestamp();
        var filename = Path.Combine("_STATE_FILES", "NttWorld.json");

        if (!File.Exists(filename))
        {
            NTTs = [];
            return;
        }

        if (File.Exists("_STATE_FILES/tick.last"))
            Tick = long.Parse(File.ReadAllText("_STATE_FILES/tick.last"));

        var json = File.ReadAllText(filename);
        NTTs = JsonSerializer.Deserialize<Dictionary<Guid, NTT>>(json) ?? [];
        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Loaded NttWorld in {time}ms");
    }

    /// <summary>
    /// Registers the array of systems to process entities each tick.
    /// </summary>
    /// <param name="systems">Array of systems to register for entity processing</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetSystems(params NttSystem[] systems) => Systems = systems;

    /// <summary>
    /// Sets the target ticks per second for consistent simulation timing.
    /// </summary>
    /// <param name="fps">Target frames/ticks per second</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTPS(int fps) => TargetTps = fps;

    /// <summary>
    /// Registers a callback to be invoked every second.
    /// </summary>
    /// <param name="action">Action to invoke every second</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RegisterOnSecond(Action action) => OnSecond += action;

    /// <summary>
    /// Registers a callback to be invoked at the end of each tick.
    /// </summary>
    /// <param name="action">Action to invoke at tick end</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RegisterOnEndTick(Action action) => OnEndTick += action;

    /// <summary>
    /// Registers a callback to be invoked at the beginning of each tick.
    /// </summary>
    /// <param name="action">Action to invoke at tick start</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RegisterOnBeginTick(Action action) => OnBeginTick += action;

    /// <summary>
    /// Creates a new entity with the specified GUID.
    /// </summary>
    /// <param name="id">GUID for the entity</param>
    /// <returns>Reference to the newly created entity</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref NTT CreateEntity(Guid id)
    {
        var ntt = new NTT(id);
        lock (NTTs)
            NTTs.Add(ntt, ntt);
        return ref CollectionsMarshal.GetValueRefOrNullRef(NTTs, id);
    }

    /// <summary>
    /// Creates a new entity with an automatically generated GUID.
    /// </summary>
    /// <returns>Reference to the newly created entity</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref NTT CreateEntity()
    {
        var id = Guid.NewGuid();
        var ntt = new NTT(id);
        lock (NTTs)
            NTTs.Add(ntt, ntt);
        return ref CollectionsMarshal.GetValueRefOrNullRef(NTTs, id);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref NTT GetEntity(Guid nttId) => ref NTTs.ContainsKey(nttId) ? ref CollectionsMarshal.GetValueRefOrNullRef(NTTs, nttId) : ref Default[0];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EntityExists(Guid nttId) => NTTs.ContainsKey(nttId);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InformChangesFor(NTT ntt) => ChangedThisTick.Enqueue(ntt);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Destroy(NTT ntt) => ToBeRemoved.Enqueue(ntt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DestroyInternal(NTT ntt)
    {
        Players.Remove(ntt);
        ntt.Recycle();
        ChangedThisTick.Enqueue(ntt);
        NTTs.Remove(ntt.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateNTTs()
    {
        while (ToBeRemoved.TryDequeue(out var ntt))
            DestroyInternal(ntt);

        while (ChangedThisTick.TryDequeue(out var ntt))
        {
            foreach (var system in Systems)
                system.EntityChanged(ntt);
        }
    }
    /// <summary>
    /// Main game loop update handling timing, system processing, and frame rate management.
    /// Maintains consistent tick rate and processes all registered systems each frame.
    /// </summary>
    public static void Update()
    {
        var tickTime = Stopwatch.GetElapsedTime(TickBeginTime);
        TickBeginTime = Stopwatch.GetTimestamp();
        var dt = MathF.Min(1f / TargetTps, (float)tickTime.TotalSeconds);
        TimeAcc += dt;
        UpdateTimeAcc += dt;

        if (UpdateTimeAcc >= UpdateTime)
        {
            UpdateTimeAcc -= UpdateTime;

            OnBeginTick?.Invoke();

            for (var i = 0; i < Systems.Length; i++)
            {
                UpdateNTTs();
                Systems[i].BeginUpdate(UpdateTime);
            }
            UpdateNTTs();

            OnEndTick?.Invoke();
            Tick++;

            if (TimeAcc < 1)
                return;

            OnSecond?.Invoke();
            TimeAcc = 0;
        }

        var tickDuration = (float)Stopwatch.GetElapsedTime(TickBeginTime).TotalMilliseconds;
        var sleepTime = (int)Math.Max(0, -1 + UpdateTime * 1000 - tickDuration);
        Thread.Sleep(sleepTime);
    }
}