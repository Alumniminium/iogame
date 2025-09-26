using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Threading;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;
using server.Simulation.Systems;

namespace server.Simulation;

public static class Game
{
    public static readonly Vector2 MapSize = new(1_500, 10_000);

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

            new NetSyncSystem(), // Check for position changes BEFORE syncing
            new Box2DEngineSystem(),
            new EnergySystem(),
            new ShieldSystem(),
            new WeaponSystem(),

            new PickupCollisionResolver(),
            new ProjectileCollisionSystem(),
            new DamageSystem(),
            new HealthSystem(),
            new DropSystem(),

            // Asteroid systems - ORDER MATTERS!
            new AsteroidNeighborTrackingSystem(),    // Must run before integrity
            new AsteroidStructuralIntegritySystem(), // Must run before collapse
            new AsteroidCollapseSystem(),           // Must run before death

            // Ship systems
            // new ShipPropulsionSystem(),             // Handle thrust from engines

            new DeathSystem(),
            new LevelExpSystem(),
            new RespawnSystem(),
            new LifetimeSystem(),
            new CleanupSystem()
        };
        NttWorld.SetSystems(systems.ToArray());
        NttWorld.SetTPS(60);
        Box2DPhysicsWorld.CreateMapBorders(MapSize);

        // Create test asteroid near player spawn
        var asteroidCenter = new Vector2(MapSize.X / 2 - 50, MapSize.Y - 50);
        var hollowSize = new Vector2(10, 10); // 10x10 spawn area
        SpawnManager.CreateAsteroid(asteroidCenter, 60, hollowSize, 12345);

        var worker = new Thread(GameLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
        worker.Start();
    }


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

            // Step physics at 120hz
            while (physicsTimeAcc >= physicsDeltaTime)
            {
                physicsTimeAcc -= physicsDeltaTime;
                Box2DPhysicsWorld.Step(physicsDeltaTime);
            }

            if (updateTimeAcc >= updateTime)
            {
                updateTimeAcc -= updateTime;

                IncomingPacketQueue.ProcessAll();

                // Update ECS systems
                NttWorld.UpdateSystems();

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
    public static void Broadcast(Memory<byte> packet)
    {
        foreach (var ntt in NttWorld.Players)
            ntt.NetSync(packet);
    }
}