using System;
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
        public static readonly Vector2 MapSize = new(10_000, 10_000);
        public static readonly Grid Grid = new((int)MapSize.X, (int)MapSize.Y, 10, 10);
        public const int TargetTps = 60;
        private const string SLEEP = "Sleep";
        private const string WORLD_UPDATE = "World.Update";

        public static uint CurrentTick { get; private set; }
        public static uint TicksPerSecond { get; private set; }

        static Game()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            PixelWorld.Systems.Add(new LifetimeSystem());
            PixelWorld.Systems.Add(new SpawnSystem());
            PixelWorld.Systems.Add(new ViewportSystem());
            PixelWorld.Systems.Add(new BoidSystem());
            PixelWorld.Systems.Add(new InputSystem());
            PixelWorld.Systems.Add(new WeaponSystem());
            PixelWorld.Systems.Add(new EngineSystem());
            PixelWorld.Systems.Add(new PhysicsSystem());
            PixelWorld.Systems.Add(new CollisionDetector());
            PixelWorld.Systems.Add(new PickupCollisionResolver());
            PixelWorld.Systems.Add(new KineticCollisionResolver());
            PixelWorld.Systems.Add(new ProjectileCollisionSystem());
            PixelWorld.Systems.Add(new DamageSystem());
            PixelWorld.Systems.Add(new HealthSystem());
            PixelWorld.Systems.Add(new DropSystem());
            PixelWorld.Systems.Add(new DeathSystem());
            PixelWorld.Systems.Add(new NetSyncSystem());
            PixelWorld.Systems.Add(new CleanupSystem());
            PerformanceMetrics.RegisterSystem(WORLD_UPDATE);
            PerformanceMetrics.RegisterSystem(SLEEP);
            PerformanceMetrics.RegisterSystem(nameof(Game));
            PerformanceMetrics.RegisterSystem(nameof(GC));

            Db.LoadBaseResources();

            SpawnManager.CreateSpawner(100, 100, 3, TimeSpan.FromSeconds(1), 1, 100);
            SpawnManager.CreateSpawner(300, 100, 4, TimeSpan.FromSeconds(1), 1, 100);
            SpawnManager.CreateSpawner(100, 300, 4, TimeSpan.FromSeconds(1), 1, 100);
            SpawnManager.CreateSpawner(300, 300, 3, TimeSpan.FromSeconds(1), 1, 100);
            SpawnManager.Respawn();
            // SpawnManager.SpawnBoids(5000);
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
                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    last = sw.Elapsed.TotalMilliseconds;
                    IncomingPacketQueue.ProcessAll();
                    PerformanceMetrics.AddSample(nameof(IncomingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    for (var i = 0; i < PixelWorld.Systems.Count; i++)
                    {
                        var system = PixelWorld.Systems[i];
                        var lastSys = sw.Elapsed.TotalMilliseconds;
                        system.Update(fixedUpdateTime);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - lastSys);

                        last = sw.Elapsed.TotalMilliseconds;
                        PixelWorld.Update(false);
                        PerformanceMetrics.AddSample(WORLD_UPDATE, sw.Elapsed.TotalMilliseconds - last);
                    }

                    if (onSecond > 1)
                    {
                        PerformanceMetrics.Restart();
                        var lines = PerformanceMetrics.Draw();
                        for (var i = 0; i < PixelWorld.Players.Count; i++)
                        {
                            var ntt = PixelWorld.Players[i];
                            ntt.NetSync(PingPacket.Create());
                            foreach (var line in lines.Split(Environment.NewLine))
                            {
                                if (!string.IsNullOrEmpty(line))
                                    ntt.NetSync(ChatPacket.Create("Server", line));
                            }
                        }
                        FConsole.WriteLine(lines);

                        onSecond = 0;
                        TicksPerSecond = 0;
                    }

                    last = sw.Elapsed.TotalMilliseconds;
                    await OutgoingPacketQueue.SendAll();
                    PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    last = sw.Elapsed.TotalMilliseconds;
                    PixelWorld.Update(true);
                    PerformanceMetrics.AddSample(WORLD_UPDATE, sw.Elapsed.TotalMilliseconds - last);

                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                    PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
                }

                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                var sleepTime = (int)Math.Max(0, fixedUpdateTime * 1000 - tickTime);
                Thread.Sleep(sleepTime);
                PerformanceMetrics.AddSample(SLEEP, sw.Elapsed.TotalMilliseconds - last);
                TicksPerSecond++;
            }
        }
        public static void Broadcast(byte[] packet)
        {
            for (var i = 0; i < PixelWorld.Players.Count; i++)
            {
                var ntt = PixelWorld.Players[i];
                ntt.NetSync(in packet);
            }
        }
    }
}