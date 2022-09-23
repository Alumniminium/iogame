using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Threading;
using server.ECS;
using server.Helpers;
using server.Simulation.Database;
using server.Simulation.Managers;
using server.Simulation.Net.Packets;
using server.Simulation.SpaceParition;
using server.Simulation.Systems;

namespace server.Simulation
{
    public static class Game
    {
        public static readonly Vector2 MapSize = new(1_500, 15_000);
        public static readonly Grid Grid = new((int)MapSize.X, (int)MapSize.Y, 20, 20);
        public const int TargetTps = 60;
        public static uint CurrentTick { get; private set; }
        private const string SLEEP = "Sleep";
        private const string WORLD_UPDATE = "World.Update";

        static Game()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            var systems = new List<PixelSystem>
            {
                new SpawnSystem(),
                new LifetimeSystem(),
                new ViewportSystem(),
                new BoidSystem(),
                new InputSystem(),
                new EnergySystem(),
                new ShieldSystem(),
                new WeaponSystem(),
                new EngineSystem(),
                new PhysicsSystem(),
                new CollisionDetector(),
                new PickupCollisionResolver(),
                new BodyDamageResolver(),
                new ProjectileCollisionSystem(),
                new DamageSystem(),
                new HealthSystem(),
                new DropSystem(),
                new DeathSystem(),
                new LevelExpSystem(),
                new RespawnSystem(),
                new NetSyncSystem(),
                new CleanupSystem()
            };
            PixelWorld.Systems = systems.ToArray();
            PerformanceMetrics.RegisterSystem(WORLD_UPDATE);
            PerformanceMetrics.RegisterSystem(SLEEP);
            PerformanceMetrics.RegisterSystem(nameof(Game));

            Db.LoadBaseResources();

            // SpawnManager.CreateSpawner((int)(MapSize.X / 2.5f), (int)(MapSize.Y - 420), 3, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateSpawner((int)(MapSize.X / 3), (int)(MapSize.Y - 420), 4, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));

            // SpawnManager.CreateSpawner((int)(MapSize.X / 1.25f), (int)(MapSize.Y - 420), 5, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateSpawner((int)(MapSize.X / 1.125f), (int)(MapSize.Y - 420), 6, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));
            SpawnManager.Respawn();

            SpawnManager.CreateStructure(500, 5, new Vector2(600, MapSize.Y - 250), 20f, Convert.ToUInt32("80ED99", 16));
            SpawnManager.CreateStructure(500, 5, new Vector2(1200, MapSize.Y - 250), -20f, Convert.ToUInt32("10EFAA", 16));
            SpawnManager.CreateStructure(50, 5, new Vector2(820, MapSize.Y - 200), 0f, Convert.ToUInt32("434343", 16));
            SpawnManager.CreateStructure(50, 5, new Vector2(980, MapSize.Y - 200), 0f, Convert.ToUInt32("30ED99", 16));

            // SpawnManager.CreateStructure(500,5, new Vector2(600, MapSize.Y -300), 10f,Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateStructure(500,5, new Vector2(400, MapSize.Y -300), 10f, Convert.ToUInt32("10EFAA", 16));
            // SpawnManager.SpawnBoids(500);
            // SpawnManager.SpawnPolygon(new Vector2(MapSize.X / 2, MapSize.Y - 500));
            var worker = new Thread(GameLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };
            worker.Start();
        }

        private static async void GameLoopAsync()
        {
            var sw = Stopwatch.StartNew();
            var fixedUpdateAcc = 0f;
            const float fixedUpdateTime = 1f / TargetTps;
            var onSecond = 0f;

            while (true)
            {
                var dt = MathF.Min(1f / TargetTps, (float)sw.Elapsed.TotalSeconds);
                fixedUpdateAcc += dt;
                onSecond += dt;
                sw.Restart();

                double last;
                IncomingPacketQueue.ProcessAll();

                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    for (var i = 0; i < PixelWorld.Systems.Length; i++)
                    {
                        var system = PixelWorld.Systems[i];
                        last = sw.Elapsed.TotalMilliseconds;
                        system.Update(fixedUpdateTime);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - last);
                        last = sw.Elapsed.TotalMilliseconds;
                        PixelWorld.Update();
                        PerformanceMetrics.AddSample(WORLD_UPDATE, sw.Elapsed.TotalMilliseconds - last);
                    }


                    if (onSecond > 1)
                    {
                        PerformanceMetrics.Restart();
                        var lines = PerformanceMetrics.Draw();
                        foreach (var ntt in PixelWorld.Players)
                            ntt.NetSync(PingPacket.Create());

                        FConsole.WriteLine(lines);

                        onSecond = 0;
                    }
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                    PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
                }
                await OutgoingPacketQueue.SendAll().ConfigureAwait(false);
                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                var sleepTime = (int)Math.Max(0, (fixedUpdateTime * 1000) - tickTime);
                Thread.Sleep(sleepTime);
                PerformanceMetrics.AddSample(SLEEP, sw.Elapsed.TotalMilliseconds - last);
            }
        }
        public static void Broadcast(Memory<byte> packet)
        {
            foreach (var ntt in PixelWorld.Players)
                ntt.NetSync(in packet);
        }
    }
}