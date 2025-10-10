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
        CreateAsteroidField();

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
    /// Creates a 5x5 grid of asteroid blocks at position (260, 260) for mining testing.
    /// Each block is a simple collidable entity with health and physics.
    /// </summary>
    private static void CreateAsteroidField()
    {
        float centerX = MapSize.X / 2 + 20;
        float centerY = MapSize.Y / 2 + 20;
        const float spacing = 1f; // Space between blocks
        const int gridSize = 5;

        // Calculate starting position to center the grid at (260, 260)
        var startX = centerX - (gridSize - 1) * spacing / 2f;
        var startY = centerY - (gridSize - 1) * spacing / 2f;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                var x = startX + col * spacing;
                var y = startY + row * spacing;
                var position = new Vector2(x, y);

                var ntt = NttWorld.CreateEntity();

                // Create a health component (100 HP per block, can be destroyed)
                var health = new HealthComponent(100f, 100f);

                // Create a physics body (static for now, can be made dynamic if needed)
                // Using box shape, gray color, moderate density
                var bodyId = Box2DPhysicsWorld.CreateBody(
                    position,
                    rotation: 0f,
                    isStatic: false, // Dynamic so they can react to impacts
                    ShapeType.Box,
                    density: 5f, // Heavy enough to be solid
                    friction: 0.5f,
                    restitution: 0.2f
                );

                var box2DBody = new Box2DBodyComponent(
                    bodyId,
                    isStatic: false,
                    color: 0x808080, // Gray
                    density: 5f,
                    sides: 4 // Box shape
                );

                // Add drop resource so they can be mined for resources
                var dropResource = new DropResourceComponent(amount: 10);

                ntt.Set(ref health);
                ntt.Set(ref box2DBody);
                ntt.Set(ref dropResource);
            }
        }
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
                    timeAcc = 0;
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