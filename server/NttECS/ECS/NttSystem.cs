using System;
using System.Collections.Concurrent;
using server.Memory;

namespace server.ECS;

/// <summary>
/// Base class for all ECS systems providing entity filtering, multi-threading, and performance monitoring.
/// Systems process entities that match specific component requirements with configurable parallelization.
/// </summary>
public abstract class NttSystem
{
    /// <summary>Delta time between ticks in seconds </summary>
    public static float DeltaTime;
    /// <summary>Current game tick for timing calculations</summary>
    public static long Tick => NttWorld.Tick;
    /// <summary>Human-readable name for this system</summary>
    public string Name;
    /// <summary>Whether this system outputs debug logging</summary>
    public bool IsLogging;
    /// <summary>Number of threads allocated for processing entities</summary>
    public int ThreadCount;
    /// <summary>Thread-safe collection of entities matching this system's filter</summary>
    internal readonly ConcurrentDictionary<Guid, NTT> _entities = new();
    /// <summary>List view of entities for efficient iteration</summary>
    internal readonly SwapList<NTT> _entitiesList = new(64);

    /// <summary>
    /// Initializes a new system with specified threading and logging configuration.
    /// </summary>
    /// <param name="name">Human-readable system name for debugging and metrics</param>
    /// <param name="threads">Number of threads to use for parallel processing</param>
    /// <param name="log">Whether to enable debug logging for this system</param>
    protected NttSystem(string name, int threads = 1, bool log = true)
    {
        ThreadCount = threads;
        IsLogging = log;
        Name = name;
    }

    /// <summary>
    /// Initiates system update with threading decisions and performance monitoring.
    /// Determines whether to use single-threaded or multi-threaded processing based on entity count.
    /// </summary>
    public void BeginUpdate(float deltaTime)
    {
        DeltaTime = deltaTime;
        if (_entities.IsEmpty)
            return;

        if (ThreadCount > 1 && _entitiesList.Count > ThreadCount * 2)
            ThreadedWorker.Run(EndUpdate, ThreadCount);
        else
            Update(0, _entitiesList.Count);

    }

    /// <summary>
    /// Handles work distribution for multi-threaded entity processing.
    /// Calculates entity ranges for each thread ensuring all entities are processed exactly once.
    /// </summary>
    /// <param name="idx">Thread index for this worker</param>
    /// <param name="threads">Total number of worker threads</param>
    public void EndUpdate(int idx, int threads)
    {
        var totalEntities = _entitiesList.Count;

        // For small workloads, only thread 0 processes everything
        if (totalEntities <= threads * 2)
        {
            if (idx == 0)
                Update(0, totalEntities);
            return;
        }

        // Calculate work distribution ensuring all entities are processed
        var baseChunkSize = totalEntities / threads;
        var extraEntities = totalEntities % threads;

        // First 'extraEntities' threads get one extra entity
        var chunkSize = baseChunkSize + (idx < extraEntities ? 1 : 0);
        var start = baseChunkSize * idx + Math.Min(idx, extraEntities);

        Update(start, chunkSize);
    }

    /// <summary>
    /// Abstract method implemented by derived systems to process a range of entities.
    /// </summary>
    /// <param name="start">Starting index in entity list</param>
    /// <param name="amount">Number of entities to process</param>
    protected abstract void Update(int start, int amount);

    /// <summary>
    /// Determines if an entity should be processed by this system based on component requirements.
    /// </summary>
    /// <param name="ntint">Entity to evaluate</param>
    /// <returns>True if entity matches system requirements</returns>
    protected virtual bool MatchesFilter(in NTT ntint) => !ntint.Id.Equals(default);

    /// <summary>
    /// Handles entity addition/removal when entity components change.
    /// Maintains the filtered entity collection based on current component state.
    /// </summary>
    /// <param name="ntt">Entity that has changed</param>
    public void EntityChanged(in NTT ntt)
    {
        var isMatch = MatchesFilter(in ntt);
        if (!isMatch)
        {
            if (_entities.TryRemove(ntt, out _))
                _entitiesList.Remove(ntt);
        }
        else
        {
            if (_entities.TryAdd(ntt, ntt))
                _entitiesList.Add(ntt);
        }
    }
}
/// <summary>
/// Generic system base class for processing entities with exactly one component type.
/// Provides type-safe access to component data with automatic filtering.
/// </summary>
/// <typeparam name="T">Required component type for entity filtering</typeparam>
/// <param name="name">System name for debugging and metrics</param>
/// <param name="threads">Number of processing threads</param>
/// <param name="log">Enable debug logging</param>
public abstract class NttSystem<T>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        // Fallback to entity-based iteration
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            Update(in ntt, ref c1);
        }
    }

    /// <summary>
    /// Processes a single entity with its component. Must be implemented by derived systems.
    /// </summary>
    /// <param name="ntt">Entity to process</param>
    /// <param name="c1">Reference to the entity's component</param>
    public abstract void Update(in NTT ntt, ref T c1);
}
/// <summary>
/// Generic system base class for processing entities with exactly two component types.
/// Uses entity-based iteration but benefits from packed storage through faster Get<T>() calls.
/// TODO Phase 2: Implement multi-component packed iteration for maximum performance.
/// </summary>
/// <typeparam name="T">First required component type</typeparam>
/// <typeparam name="T2">Second required component type</typeparam>
public abstract class NttSystem<T, T2>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T, T2>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            ref var c2 = ref ntt.Get<T2>();
            Update(in ntt, ref c1, ref c2);
        }
    }
    /// <summary>
    /// Processes a single entity with its two components.
    /// </summary>
    /// <param name="ntt">Entity to process</param>
    /// <param name="c1">Reference to first component</param>
    /// <param name="c2">Reference to second component</param>
    public abstract void Update(in NTT ntt, ref T c1, ref T2 c2);
}
public abstract class NttSystem<T, T2, T3>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T, T2, T3>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            ref var c2 = ref ntt.Get<T2>();
            ref var c3 = ref ntt.Get<T3>();
            Update(in ntt, ref c1, ref c2, ref c3);
        }
    }
    public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3);
}
public abstract class NttSystem<T, T2, T3, T4>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T, T2, T3, T4>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            ref var c2 = ref ntt.Get<T2>();
            ref var c3 = ref ntt.Get<T3>();
            ref var c4 = ref ntt.Get<T4>();
            Update(in ntt, ref c1, ref c2, ref c3, ref c4);
        }
    }
    public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4);
}
public abstract class NttSystem<T, T2, T3, T4, T5>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T, T2, T3, T4, T5>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            ref var c2 = ref ntt.Get<T2>();
            ref var c3 = ref ntt.Get<T3>();
            ref var c4 = ref ntt.Get<T4>();
            ref var c5 = ref ntt.Get<T5>();
            Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5);
        }
    }
    public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5);
}
public abstract class NttSystem<T, T2, T3, T4, T5, T6>(string name, int threads = 1, bool log = false) : NttSystem(name, threads, log) where T : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
{
    protected override bool MatchesFilter(in NTT ntint) => ntint.Has<T, T2, T3, T4, T5, T6>() && base.MatchesFilter(in ntint);

    protected override void Update(int start, int amount)
    {
        var span = _entitiesList.AsSpan(start, amount);
        for (var i = 0; i < span.Length; i++)
        {
            ref readonly var ntt = ref span[i];
            ref var c1 = ref ntt.Get<T>();
            ref var c2 = ref ntt.Get<T2>();
            ref var c3 = ref ntt.Get<T3>();
            ref var c4 = ref ntt.Get<T4>();
            ref var c5 = ref ntt.Get<T5>();
            ref var c6 = ref ntt.Get<T6>();
            Update(in ntt, ref c1, ref c2, ref c3, ref c4, ref c5, ref c6);
        }
    }
    public abstract void Update(in NTT ntt, ref T c1, ref T2 c2, ref T3 c3, ref T4 c4, ref T5 c5, ref T6 c6);
}