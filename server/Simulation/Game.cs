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
        public static readonly int UPDATE_RATE_MS = 33;

        public const int MAP_WIDTH = 2000;
        public const int MAP_HEIGHT = 2000;

        public static uint CurrentTick {get;private set;}
        public static uint TicksPerSecond {get;private set;}

        private static readonly Thread worker;
        public static readonly Random Random = new();

        static readonly TimedThing[] _timedThings = new TimedThing[]
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
                    // pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond} | Entities: {World.ShapeEntities.Count}"));
                }

                // FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS}");
                TicksPerSecond = 0;
            })
        };

        static Game()
        {
            World.Systems.Add(new GCMonitor());
            World.Systems.Add(new BoidSystem());
            World.Systems.Add(new InputSystem());
            World.Systems.Add(new MoveSystem());
            World.Systems.Add(new HealthSystem());
            World.Systems.Add(new LifetimeSystem());
            PerformanceMetrics.RegisterSystem(nameof(_timedThings));
            PerformanceMetrics.RegisterSystem("World.Update");
            PerformanceMetrics.RegisterSystem("Grid.Clear");
            PerformanceMetrics.RegisterSystem("Grid.Insert");
            PerformanceMetrics.RegisterSystem("Sleep");
            PerformanceMetrics.RegisterSystem(nameof(Game));
            
            Db.LoadBaseResources();
            // SpawnManager.Respawn();
            SpawnManager.SpawnBoids(250);
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

                // if (fixedUpdateAcc >= fixedUpdateTime)
                // {
                    foreach (var system in World.Systems)
                    {
                        var lastSys = sw.Elapsed.TotalMilliseconds;
                        system.Update(dt);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - lastSys);
                        last = sw.Elapsed.TotalMilliseconds;
                        World.Update();
                        PerformanceMetrics.AddSample("World.Update", sw.Elapsed.TotalMilliseconds - last);
                    }
                    CollisionDetection.Process(dt);
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                // }

                last = sw.Elapsed.TotalMilliseconds;
                for (int i = 0; i < _timedThings.Length; i++)
                    _timedThings[i].Update(dt);
                PerformanceMetrics.AddSample(nameof(_timedThings), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                await OutgoingPacketQueue.SendAll();
                PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, fixedUpdateTime * 1000 - tickTime)));
                PerformanceMetrics.AddSample("Sleep", sw.Elapsed.TotalMilliseconds - last);
                TicksPerSecond++;
                PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
            }
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in World.Players)
                kvp.Value.Send(packet);
        }
    }
}