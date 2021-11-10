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
        public const int TARGET_TPS = 60;
        public const int UPDATE_RATE_MS = 16;

        public const int MAP_WIDTH = 90_000;
        public const int MAP_HEIGHT = 30_000;

        public static uint CurrentTick;
        private static uint TicksPerSecond = 0;

        private static readonly Thread worker;
        public static readonly Random Random = new();

        public static TimedThing[] TimedThings = new TimedThing[]
        {
            new TimedThing(TimeSpan.FromMilliseconds(UPDATE_RATE_MS), ()=>
            {
                foreach (var pkvp in World.Players)
                {
                    var player = pkvp.Value;
                    player.Viewport.Update();
                }
            }),
            new TimedThing(TimeSpan.FromSeconds(1), ()=> {
                PerformanceMetrics.Restart();
                PerformanceMetrics.Draw();
                foreach (var pkvp in World.Players)
                {
                    pkvp.Value.Send(PingPacket.Create());
                    pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond} | Entities: {World.ShapeEntities.Count}"));
                }

                FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS}");
                TicksPerSecond = 0;
            })
        };

        static Game()
        {
            World.Systems.Add(new GCMonitor());
            World.Systems.Add(new InputSystem());
            World.Systems.Add(new MoveSystem());
            World.Systems.Add(new HealthSystem());
            World.Systems.Add(new LifetimeSystem());
            PerformanceMetrics.RegisterSystem(nameof(TimedThings));
            PerformanceMetrics.RegisterSystem("FixedUpdate");
            PerformanceMetrics.RegisterSystem("Grid.Clear");
            PerformanceMetrics.RegisterSystem("Grid.Insert");
            PerformanceMetrics.RegisterSystem("GridMove");

            Db.LoadBaseResources();
            SpawnManager.Respawn();
            worker = new Thread(GameLoopAsync) { IsBackground = true };
            worker.Start();
            GC.Collect(2, GCCollectionMode.Forced, true);
        }

        public static async void GameLoopAsync()
        {
            FConsole.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var sw = new Stopwatch();
            var fixedUpdateAcc = 0f;
            var fixedUpdateTime = 1f / TARGET_TPS;
            double last = 0;

            while (true)
            {
                var dt = (float)sw.Elapsed.TotalSeconds;
                fixedUpdateAcc += dt;
                sw.Restart();


                last = sw.Elapsed.TotalMilliseconds;
                IncomingPacketQueue.ProcessAll();
                PerformanceMetrics.AddSample(nameof(IncomingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                World.Update();

                last = sw.Elapsed.TotalMilliseconds;
                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    foreach (var system in World.Systems)
                    {
                        system.Update(fixedUpdateTime);
                        World.Update();
                    }
                    CollisionDetection.Process(dt);
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                }

                PerformanceMetrics.AddSample("FixedUpdate", sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                for (int i = 0; i < TimedThings.Length; i++)
                    TimedThings[i].Update(dt);
                PerformanceMetrics.AddSample(nameof(TimedThings), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                await OutgoingPacketQueue.SendAll();
                PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                var tickTime = sw.Elapsed.TotalMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, fixedUpdateTime * 1000 - tickTime)));
                TicksPerSecond++;
            }
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in World.Players)
                kvp.Value.Send(packet);
        }
    }
}