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
    public static readonly Vector2 MapSize = new(1500, 100_000);
    public const int TargetTps = 60;
    public const string WORLD_UPDATE = "World.Update";
    public const string SLEEP = "Sleep";

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
            new Box2DSyncSystem(), // Sync positions from Box2D
            new Box2DEngineSystem(),
            new EnergySystem(),
            new ShieldSystem(),
            new WeaponSystem(),

            new PickupCollisionResolver(),
            new ProjectileCollisionSystem(),
            new DamageSystem(),
            new HealthSystem(),
            new DropSystem(),
            new DeathSystem(),
            new LevelExpSystem(),
            new RespawnSystem(),
            new LifetimeSystem(),
            new CleanupSystem()
        };
        NttWorld.SetSystems(systems.ToArray());
        NttWorld.SetTPS(30);
        Db.BaseResources.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value.Sides}x{x.Value.Size}px"));

        SpawnManager.CreateSpawner((int)(MapSize.X / 4.5f), (int)(MapSize.Y - 420), 3, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));
        SpawnManager.CreateSpawner((int)(MapSize.X / 3.5), (int)(MapSize.Y - 420), 4, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));

        SpawnManager.CreateSpawner((int)(MapSize.X / 1.25f), (int)(MapSize.Y - 420), 5, TimeSpan.FromMilliseconds(1500), 1, 200, Convert.ToUInt32("80ED99", 16));
        SpawnManager.CreateSpawner((int)(MapSize.X / 1.125f), (int)(MapSize.Y - 420), 6, TimeSpan.FromMilliseconds(1500), 1, 200, Convert.ToUInt32("80ED99", 16));
        SpawnManager.Respawn();

        SpawnManager.CreateStructure(500, 5, new Vector2(300, MapSize.Y - 250), 15, Convert.ToUInt32("80ED99", 16), ShapeType.Box);
        SpawnManager.CreateStructure(500, 5, new Vector2(1200, MapSize.Y - 250), -15f, Convert.ToUInt32("10EFAA", 16), ShapeType.Box);
        SpawnManager.CreateStructure(50, 5, new Vector2(520, MapSize.Y - 250), 75f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        SpawnManager.CreateStructure(50, 5, new Vector2(980, MapSize.Y - 250), 115f, Convert.ToUInt32("30ED99", 16), ShapeType.Box);

        SpawnManager.CreateStructure(25, 250, new Vector2(MapSize.X / 2, MapSize.Y - 125), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 190), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 200), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 210), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        var dome = SpawnManager.CreateStructure(50, 50, new Vector2(MapSize.X / 2, MapSize.Y - 240), 0f, Convert.ToUInt32("434343", 16), ShapeType.Circle);
        var shield = new ShieldComponent(dome, 100, 10000, 0, 15, 12, 10, TimeSpan.FromSeconds(4));
        var energy = new EnergyComponent(dome, 100, 100, 1000);
        // dome.Set(ref shield);
        dome.Set(ref energy);


        // SpawnManager.CreateStructure((int)MapSize.X, 2, new Vector2(MapSize.X / 2, MapSize.Y), 0f, Convert.ToUInt32("30ED99", 16), ShapeType.Box);

        // Create map boundary walls
        CreateMapBoundaries();

        var worker = new Thread(GameLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
        worker.Start();
    }

    private static void CreateMapBoundaries()
    {
        const float wallThickness = 100f;
        const uint wallColor = 0x404040; // Dark gray

        // Left wall
        SpawnManager.CreateStructure((int)wallThickness, (int)MapSize.Y,
            new Vector2(-wallThickness / 2, MapSize.Y / 2), 0f, wallColor, ShapeType.Box);

        // Right wall
        SpawnManager.CreateStructure((int)wallThickness, (int)MapSize.Y,
            new Vector2(MapSize.X + wallThickness / 2, MapSize.Y / 2), 0f, wallColor, ShapeType.Box);

        // Top wall
        SpawnManager.CreateStructure((int)MapSize.X + (int)wallThickness * 2, (int)wallThickness,
            new Vector2(MapSize.X / 2, -wallThickness / 2), 0f, wallColor, ShapeType.Box);

        // Bottom wall
        SpawnManager.CreateStructure((int)MapSize.X + (int)wallThickness * 2, (int)wallThickness,
            new Vector2(MapSize.X / 2, MapSize.Y + wallThickness / 2), 0f, wallColor, ShapeType.Box);
    }

    private static void GameLoop()
    {
        const float deltaTime = 1f / TargetTps;
        const float physicsHz = 60f;
        const float physicsDeltaTime = 1f / physicsHz;

        var tickBeginTime = Stopwatch.GetTimestamp();
        var timeAcc = 0f;
        var updateTimeAcc = 0f;
        var physicsTimeAcc = 0f;
        const float updateTime = deltaTime;

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