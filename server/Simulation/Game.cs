using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Database;
using iogame.Simulation.Systems;
using iogame.Util;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 30;
        public const int UPDATE_RATE_MS = 33;

        public const int MAP_WIDTH = 90_000;
        public const int MAP_HEIGHT = 30_000;
        public const float DRAG = 0.99997f;

        public static uint CurrentTick;
        private static uint TicksPerSecond = 0;

        private static readonly Thread worker;
        public static readonly Random Random = new();

        public static TimedThing[] TimedThings = new TimedThing[]
        {
            new TimedThing(TimeSpan.FromMilliseconds(UPDATE_RATE_MS), ()=>
            {
                foreach (var pkvp in EntityManager.Players)
                {
                    var player = pkvp.Value;
                    player.Viewport.Update();
                }
            }),
            new TimedThing(TimeSpan.FromSeconds(1), ()=> {
                PerformanceMetrics.Restart();
                PerformanceMetrics.Draw();
                foreach (var pkvp in EntityManager.Players)
                {
                    pkvp.Value.Send(PingPacket.Create());
                    pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond} | Entities: {EntityManager.Entities.Count}"));
                }

                FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS}");
                TicksPerSecond = 0;
            })
        };

        static Game()
        {
            PerformanceMetrics.RegisterSystem(nameof(TimedThings));
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

                last = sw.Elapsed.TotalMilliseconds;
                EntityManager.Update();
                PerformanceMetrics.AddSample(nameof(EntityManager), sw.Elapsed.TotalMilliseconds - last);

                while (fixedUpdateAcc >= fixedUpdateTime)
                {
                    FixedUpdate(fixedUpdateTime, sw);
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                }

                last = sw.Elapsed.TotalMilliseconds;
                for (int i = 0; i < TimedThings.Length; i++)
                    TimedThings[i].Update(dt);
                PerformanceMetrics.AddSample(nameof(TimedThings), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                await OutgoingPacketQueue.SendAll();
                PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), sw.Elapsed.TotalMilliseconds - last);

                var tickTime = sw.Elapsed.TotalMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, fixedUpdateTime/3 * 1000 - tickTime)));
                TicksPerSecond++;
            }
        }
        private static void FixedUpdate(float dt, Stopwatch sw)
        {
            var last = sw.Elapsed.TotalMilliseconds;
            CollisionDetection.Grid.Clear();
            PerformanceMetrics.AddSample("Grid.Clear", sw.Elapsed.TotalMilliseconds - last);

            foreach (var kvp in EntityManager.Entities)
            {
                var entity = kvp.Value;

                last = sw.Elapsed.TotalMilliseconds;
                LifetimeSystem.Update(dt, entity);
                PerformanceMetrics.AddSample(nameof(LifetimeSystem), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                MoveSystem.Update(dt, entity);
                PerformanceMetrics.AddSample(nameof(MoveSystem), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                RotationSystem.Update(dt, entity);
                PerformanceMetrics.AddSample(nameof(RotationSystem), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                HealthSystem.Update(dt, entity);
                PerformanceMetrics.AddSample(nameof(HealthSystem), sw.Elapsed.TotalMilliseconds - last);

                last = sw.Elapsed.TotalMilliseconds;
                CollisionDetection.Grid.Insert(entity);
                PerformanceMetrics.AddSample("Grid.Insert", sw.Elapsed.TotalMilliseconds - last);
            }

            last = sw.Elapsed.TotalMilliseconds;
            CollisionDetection.Process(dt);
            PerformanceMetrics.AddSample(nameof(CollisionDetection), sw.Elapsed.TotalMilliseconds - last);
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in EntityManager.Players)
                kvp.Value.Send(packet);
        }
    }
}