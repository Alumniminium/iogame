using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

/// <summary>
/// Processes player input and configures entity components (engines, weapons, shields, inventory) based on button states.
/// Translates raw input flags into component state changes for subsequent systems to process.
/// </summary>
public sealed class InputSystem : NttSystem<InputComponent>
{
    public InputSystem() : base("Input System", threads: 1) { }

    public override void Update(in NTT ntt, ref InputComponent c1)
    {
        Console.WriteLine($"[InputSystem] Processing entity {ntt.Id}, buttonStates={c1.ButtonStates}, hasEngine={ntt.Has<EngineComponent>()}");

        if (ntt.Has<EngineComponent>())
            ConfigureEngine(in ntt, ref c1);
        if (ntt.Has<WeaponComponent>())
            ConfigureWeapons(in ntt, ref c1);
        if (ntt.Has<InventoryComponent>())
            ConfigureInventory(in ntt, ref c1);
        if (ntt.Has<ShieldComponent>())
            ConfigureShield(in ntt, ref c1);
    }

    /// <summary>
    /// Configures shield power state based on player input.
    /// </summary>
    private static void ConfigureShield(in NTT ntt, ref InputComponent c1)
    {
        ref var shield = ref ntt.Get<ShieldComponent>();
        shield.PowerOn = c1.ButtonStates.HasFlag(PlayerInput.Shield);
        if (shield.LastPowerOn == shield.PowerOn)
            return;
        shield.LastPowerOn = shield.PowerOn;
        shield.ChangedTick = NttWorld.Tick;
    }

    /// <summary>
    /// Sets weapon fire state when fire button is pressed.
    /// </summary>
    private static void ConfigureWeapons(in NTT ntt, ref InputComponent inp)
    {
        if (!inp.ButtonStates.HasFlag(PlayerInput.Fire))
            return;

        ref var wep = ref ntt.Get<WeaponComponent>();
        wep.Fire = true;
    }

    /// <summary>
    /// Handles item dropping from inventory when drop button is pressed.
    /// Spawns dropped items behind the entity with random spread and velocity.
    /// </summary>
    private static void ConfigureInventory(in NTT ntt, ref InputComponent inp)
    {
        if (!inp.ButtonStates.HasFlag(PlayerInput.Drop))
            return;

        ref var rigidBody = ref ntt.Get<Box2DBodyComponent>();
        ref readonly var phy = ref ntt.Get<Box2DBodyComponent>();
        ref var wep = ref ntt.Get<WeaponComponent>();
        var halfPi = MathF.PI / 2;
        var forward = new Vector2(MathF.Cos(rigidBody.RotationRadians), MathF.Sin(rigidBody.RotationRadians));
        var behind = -forward.ToRadians();

        behind += (Random.Shared.NextSingle() + -Random.Shared.NextSingle()) * halfPi;

        var dx = MathF.Cos(behind);
        var dy = MathF.Sin(behind);

        var dropX = -dx + rigidBody.Position.X;
        var dropY = -dy + rigidBody.Position.Y;
        var dropPos = new Vector2(dropX, dropY);

        var dist = rigidBody.Position - dropPos;
        var penDepth = 0.5f + 1 - dist.Length(); // Fixed 1x1 entity radius
        var penRes = Vector2.Normalize(dist) * penDepth * 1.25f;
        dropPos += penRes;

        if (dropPos.X + 1 > Game.MapSize.X || dropPos.X - 1 < 0 || dropPos.Y + 1 > Game.MapSize.Y || dropPos.Y - 1 < 0)
            return;

        var velocity = new Vector2(dx, dy) * 10;

        ref var inv = ref ntt.Get<InventoryComponent>();
        if (inv.Triangles != 0)
        {
            inv.ChangedTick = NttWorld.Tick;
            inv.Triangles--;
            SpawnManager.SpawnDrop(Database.Db.BaseResources[3], dropPos, TimeSpan.FromSeconds(15), velocity);
        }
        if (inv.Squares != 0)
        {
            inv.ChangedTick = NttWorld.Tick;
            inv.Squares--;
            SpawnManager.SpawnDrop(Database.Db.BaseResources[4], dropPos, TimeSpan.FromSeconds(15), velocity);
        }
        if (inv.Pentagons != 0)
        {
            inv.ChangedTick = NttWorld.Tick;
            inv.Pentagons--;
            SpawnManager.SpawnDrop(Database.Db.BaseResources[5], dropPos, TimeSpan.FromSeconds(15), velocity);
        }

    }

    /// <summary>
    /// Configures engine throttle and RCS state based on thrust, boost, and RCS input.
    /// Handles smooth throttle ramping and instant boost activation.
    /// </summary>
    private static void ConfigureEngine(in NTT ntt, ref InputComponent inp)
    {
        ref var eng = ref ntt.Get<EngineComponent>();
        eng.RCS = inp.ButtonStates.HasFlag(PlayerInput.RCS);

        if (inp.DidBoostLastFrame)
        {
            eng.ChangedTick = NttWorld.Tick;
            eng.Throttle = 0;
            inp.DidBoostLastFrame = false;
        }

        if (inp.ButtonStates.HasFlag(PlayerInput.Boost))
        {
            eng.ChangedTick = NttWorld.Tick;
            eng.Throttle = 1;
            inp.DidBoostLastFrame = true;
        }
        else if (inp.ButtonStates.HasFlag(PlayerInput.Thrust))
        {
            eng.ChangedTick = NttWorld.Tick;
            eng.Throttle = Math.Clamp(eng.Throttle + (1f * DeltaTime), 0, 1);
        }
        else if (inp.ButtonStates.HasFlag(PlayerInput.InvThrust))
        {
            eng.ChangedTick = NttWorld.Tick;
            eng.Throttle = Math.Clamp(eng.Throttle - (1f * DeltaTime), 0, 1);
        }
    }
}