using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Threading;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;
using server.Simulation.Systems;

namespace server.Simulation;

/// <summary>
/// Main game coordinator managing world initialization, system registration, and the primary game loop.
/// Handles entity lifecycle, physics simulation, and network packet processing at a fixed tick rate.
/// </summary>
public static class Game
{
    /// <summary>World bounds in game units (width, height)</summary>
    public static readonly Vector2 MapSize = new(32_000, 32_000);

    static Game()
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        PerformanceMetrics.RegisterSystem("World.Update");
        PerformanceMetrics.RegisterSystem("Sleep");
        PerformanceMetrics.RegisterSystem(nameof(Game));
        var systems = new List<NttSystem>
        {
            new SpawnSystem(),
            new ViewportSystem(),
            new InputSystem(),

            new PositionSyncSystem(), // Position/movement sync with tick for client prediction
            new ShipPhysicsRebuildSystem(), // Rebuild physics bodies when ship parts change
            new GravitySystem(),
            new Box2DEngineSystem(),
            new EnergySystem(),
            new ShieldSystem(),
            new WeaponSystem(),

            new PickupCollisionResolver(),
            new ProjectileCollisionSystem(),
            new DamageSystem(),
            new HealthSystem(),
            new DropSystem(),

            new LifetimeSystem(),
            new LevelExpSystem(),
            new RespawnSystem(),
            new ComponentSyncSystem(), // Generic component sync system
            new DeathSystem(),
        };
        NttWorld.SetSystems(systems.ToArray());
        NttWorld.SetTPS(60);
        Box2DPhysicsWorld.CreateMapBorders(MapSize);

        CreateGravitySources();

        var worker = new Thread(GameLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
        worker.Start();
    }

    /// <summary>
    /// Creates gravity sources at the top and bottom edges of the map.
    /// </summary>
    private static void CreateGravitySources()
    {
        var centerX = MapSize.X / 2;

        // Top gravity source (positioned below center, pulls downward toward top edge)
        var topGravityNtt = NttWorld.CreateEntity();
        var topBody = Box2DPhysicsWorld.CreateBody(
            new Vector2(centerX, MapSize.Y + 1),
            0f,
            isStatic: true,
            ShapeType.Circle,
            density: 1f
        );
        var topBox2DBody = new Box2DBodyComponent(topBody, true, 0xFF0000);
        topGravityNtt.Set(topBox2DBody);
        var topGravity = new GravityComponent(strength: 9.81f, radius: 100f);
        topGravity.ChangedTick = NttWorld.Tick;
        topGravityNtt.Set(topGravity);

        // Bottom gravity source (positioned above center, pulls upward toward bottom edge)
        var bottomGravityNtt = NttWorld.CreateEntity();
        var bottomBody = Box2DPhysicsWorld.CreateBody(
            new Vector2(centerX, -1),
            0f,
            isStatic: true,
            ShapeType.Circle,
            density: 1f
        );
        var bottomBox2DBody = new Box2DBodyComponent(bottomBody, true, 0x00FF00);
        bottomGravityNtt.Set(bottomBox2DBody);
        var bottomGravity = new GravityComponent(strength: 9.81f, radius: 100f);
        bottomGravity.ChangedTick = NttWorld.Tick;
        bottomGravityNtt.Set(bottomGravity);
    }


    /// <summary>
    /// Primary game loop executing at fixed tick rate with separate physics and game logic timing.
    /// Processes incoming packets, updates all systems, and sends outgoing packets each tick.
    /// </summary>
    private static void GameLoop()
    {
        float deltaTime = 1f / NttWorld.TargetTps;
        const float physicsHz = 60f;
        const float physicsDeltaTime = 1f / physicsHz;

        var tickBeginTime = Stopwatch.GetTimestamp();
        var timeAcc = 0f;
        var updateTimeAcc = 0f;
        var physicsTimeAcc = 0f;
        float updateTime = deltaTime;

        while (true)
        {
            var tickTime = Stopwatch.GetElapsedTime(tickBeginTime);
            tickBeginTime = Stopwatch.GetTimestamp();
            var dt = MathF.Min(deltaTime, (float)tickTime.TotalSeconds);
            timeAcc += dt;
            updateTimeAcc += dt;
            physicsTimeAcc += dt;

            while (physicsTimeAcc >= physicsDeltaTime)
            {
                physicsTimeAcc -= physicsDeltaTime;
                Box2DPhysicsWorld.Step(physicsDeltaTime);
            }

            if (updateTimeAcc >= updateTime)
            {
                updateTimeAcc -= updateTime;

                IncomingPacketQueue.ProcessAll();
                NttWorld.UpdateSystems();
                OutgoingPacketQueue.SendAll();

                if (timeAcc >= 1)
                {
                    timeAcc = 0;
                }
            }

            var tickDuration = (float)Stopwatch.GetElapsedTime(tickBeginTime).TotalMilliseconds;
            var sleepTime = (int)Math.Max(0, -1 + updateTime * 1000 - tickDuration);
            Thread.Sleep(sleepTime);
        }
    }
    /// <summary>
    /// Broadcasts a binary packet to all connected players via WebSocket.
    /// </summary>
    /// <param name="packet">Binary packet data to broadcast</param>
    public static void Broadcast(Memory<byte> packet)
    {
        foreach (var ntt in NttWorld.Players)
            ntt.NetSync(packet);
    }
}