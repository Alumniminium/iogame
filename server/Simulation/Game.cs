using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Database;
using iogame.Simulation.Managers;
using iogame.Simulation.Systems;
using iogame.Util;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 48;

        public const int MAP_WIDTH = 4500;
        public const int MAP_HEIGHT = 1500;

        public static uint CurrentTick { get; private set; }
        public static uint TicksPerSecond { get; private set; }

        private static readonly Thread worker;

        static Game()
        {
            PixelWorld.Systems.Add(new GCMonitor());
            PixelWorld.Systems.Add(new ViewportSystem());
            PixelWorld.Systems.Add(new BoidSystem());
            PixelWorld.Systems.Add(new InputSystem());
            PixelWorld.Systems.Add(new MoveSystem());
            PixelWorld.Systems.Add(new HealthSystem());
            PixelWorld.Systems.Add(new LifetimeSystem());
            PerformanceMetrics.RegisterSystem("World.Update");
            PerformanceMetrics.RegisterSystem("Grid.Insert");
            PerformanceMetrics.RegisterSystem("Sleep");
            PerformanceMetrics.RegisterSystem(nameof(Game));

            Db.LoadBaseResources();
            // SpawnManager.Respawn();
            SpawnManager.SpawnBoids(500);
            worker = new Thread(GameLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };
            worker.Start();
        }

        public static async void GameLoopAsync()
        {
            var sw = Stopwatch.StartNew();
            var fixedUpdateAcc = 0f;
            var fixedUpdateTime = 1f / TARGET_TPS;
            double last = 0;

            var onSecond = 0f;

            while (true)
            {
                var dt = (float)Math.Min(1f / TARGET_TPS, (float)sw.Elapsed.TotalSeconds);
                fixedUpdateAcc += dt;
                onSecond += dt;
                sw.Restart();

                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    last = sw.Elapsed.TotalMilliseconds;
                    IncomingPacketQueue.ProcessAll();
                    PerformanceMetrics.AddSample(nameof(IncomingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    foreach (var system in PixelWorld.Systems)
                    {
                        var lastSys = sw.Elapsed.TotalMilliseconds;
                        system.Update(fixedUpdateTime);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - lastSys);
                        last = sw.Elapsed.TotalMilliseconds;
                        PixelWorld.Update();
                        PerformanceMetrics.AddSample("World.Update", sw.Elapsed.TotalMilliseconds - last);
                    }
                    CollisionDetection.Process(fixedUpdateTime);

                    if (onSecond > 1)
                    {
                        onSecond = 0;
                        PerformanceMetrics.Restart();
                        if (Debugger.IsAttached)
                            PerformanceMetrics.Draw();
                        foreach (var pkvp in PixelWorld.Players)
                        {
                            pkvp.Value.Send(PingPacket.Create());
                            //     foreach(var line in load.Split(Environment.NewLine))
                            //     {
                            //         if(!string.IsNullOrEmpty(line))
                            //         pkvp.Value.Send(ChatPacket.Create("Server", line));
                            //     }
                        }
                        FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS}");
                        TicksPerSecond = 0;
                    }

                    last = sw.Elapsed.TotalMilliseconds;
                    await OutgoingPacketQueue.SendAll();
                    PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                    PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
                }

                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, fixedUpdateTime * 1000 - tickTime)));
                PerformanceMetrics.AddSample("Sleep", sw.Elapsed.TotalMilliseconds - last);
                TicksPerSecond++;
            }
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in PixelWorld.Players)
                kvp.Value.Send(packet);
        }
    }
}