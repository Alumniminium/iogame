using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using QuadTrees;
using server.ECS;
using server.Helpers;
using server.Simulation.Database;
using server.Simulation.Entities;
using server.Simulation.Managers;
using server.Simulation.Net.Packets;
using server.Simulation.Systems;

namespace server.Simulation
{
    public static class Game
    {
        public static readonly Vector2 MapSize = new(500, 500);
        public static readonly QuadTreeRectF<ShapeEntity> Tree = new(0, 0, MapSize.X, MapSize.Y);
        public const int TargetTps = 60;
        public static uint CurrentTick { get; private set; }
        public static uint TicksPerSecond { get; private set; }

        static Game()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            // PixelWorld.Systems.Add(new OrdinanceSystem());
            PixelWorld.Systems.Add(new GcMonitor());
            PixelWorld.Systems.Add(new SpawnSystem());
            PixelWorld.Systems.Add(new LifetimeSystem());
            PixelWorld.Systems.Add(new PassiveViewportSystem());
            PixelWorld.Systems.Add(new ViewportSystem());
            PixelWorld.Systems.Add(new BoidSystem());
            PixelWorld.Systems.Add(new InputSystem());
            PixelWorld.Systems.Add(new MoveSystem());
            PixelWorld.Systems.Add(new CollisionSystem());
            PixelWorld.Systems.Add(new DamageSystem());
            PixelWorld.Systems.Add(new HealthSystem());
            PixelWorld.Systems.Add(new NetSyncSystem());
            PerformanceMetrics.RegisterSystem("World.Update");
            PerformanceMetrics.RegisterSystem("Sleep");
            PerformanceMetrics.RegisterSystem(nameof(Game));

            Db.LoadBaseResources();

            SpawnManager.CreateSpawner(100,100, 3, TimeSpan.FromSeconds(5), 10, 100, 20);
            SpawnManager.CreateSpawner(300,100, 4, TimeSpan.FromSeconds(5), 10, 100, 20);
            SpawnManager.CreateSpawner(100,300, 4, TimeSpan.FromSeconds(5), 10, 100, 20);
            SpawnManager.CreateSpawner(300,300, 3, TimeSpan.FromSeconds(5), 10, 100, 20);
            // SpawnManager.Respawn();
            // SpawnManager.SpawnBoids(200);
            var worker = new Thread(GameLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };
            worker.Start();
        }

        private static void GameLoopAsync()
        {
            var sw = Stopwatch.StartNew();
            var fixedUpdateAcc = 0f;
            const float fixedUpdateTime = 1f / TargetTps;
            var onSecond = 0f;

            while (true)
            {
                var dt = Math.Min(1f / TargetTps, (float)sw.Elapsed.TotalSeconds);
                fixedUpdateAcc += dt;
                onSecond += dt;
                sw.Restart();

                double last;
                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    last = sw.Elapsed.TotalMilliseconds;
                    PixelWorld.Update();
                    PerformanceMetrics.AddSample("World.Update", sw.Elapsed.TotalMilliseconds - last);

                    last = sw.Elapsed.TotalMilliseconds;
                    IncomingPacketQueue.ProcessAll();
                    PerformanceMetrics.AddSample(nameof(IncomingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    for (int i = 0; i < PixelWorld.Systems.Count; i++)
                    {
                        var system = PixelWorld.Systems[i];
                        var lastSys = sw.Elapsed.TotalMilliseconds;
                        system.Update(fixedUpdateTime);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - lastSys);
                    }

                    if (onSecond > 1)
                    {
                        onSecond = 0;
                        PerformanceMetrics.Restart();
                        var lines = PerformanceMetrics.Draw();
                        for (int i = 0; i < PixelWorld.Players.Count; i++)
                        {
                            var entity = PixelWorld.Players[i];
                            entity.NetSync(PingPacket.Create());
                            foreach (var line in lines.Split(Environment.NewLine))
                            {
                                if (!string.IsNullOrEmpty(line))
                                    entity.NetSync(ChatPacket.Create("Server", line));
                            }
                        }
                        FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TargetTps}");
                        TicksPerSecond = 0;
                    }

                    last = sw.Elapsed.TotalMilliseconds;
                    OutgoingPacketQueue.SendAll().GetAwaiter().GetResult();
                    PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                    PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
                }

                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                var sleepTime = (int)Math.Max(0, fixedUpdateTime * 1000 - tickTime);
                Thread.Sleep(sleepTime);
                PerformanceMetrics.AddSample("Sleep", sw.Elapsed.TotalMilliseconds - last);
                TicksPerSecond++;
            }
        }
        public static void Broadcast(byte[] packet)
        {
            for (int i = 0; i < PixelWorld.Players.Count; i++)
            {
                var entity = PixelWorld.Players[i];
                entity.NetSync(packet);
            }
        }
    }
}