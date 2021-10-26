using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Simulation.Systems;
using iogame.Util;

namespace iogame.Simulation
{
    public static class Game
    {
        public const int TARGET_TPS = 1000;
        public const int PHYSICS_TPS = 30;
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
                foreach (var pkvp in EntityManager.Players)
                {
                    pkvp.Value.Send(PingPacket.Create());
                    pkvp.Value.Send(ChatPacket.Create("Server", $"Tickrate: {TicksPerSecond} (Physics: {PHYSICS_TPS}) | Entities: {EntityManager.Entities.Count}"));
                }

                FConsole.WriteLine($"Tickrate: {TicksPerSecond}/{TARGET_TPS} (Physics: {PHYSICS_TPS})");
                TicksPerSecond = 0;
            })
        };

        static Game()
        {
            Db.LoadBaseResources();
            SpawnManager.Respawn();
            worker = new Thread(GameLoopAsync) { IsBackground = true };
            worker.Start();
        }

        public static async void GameLoopAsync()
        {
            FConsole.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var sw = new Stopwatch();
            var sleepTime = 1000 / TARGET_TPS;
            var fixedUpdateAcc = 0f;
            var fixedUpdateTime = 1f / PHYSICS_TPS;
            double last = 0;

            while (true)
            {
                var dt = (float)sw.Elapsed.TotalSeconds;
                fixedUpdateAcc += dt;
                sw.Restart();
                last = sw.Elapsed.TotalMilliseconds;
                FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
                IncomingPacketQueue.ProcessAll();
                FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Processing Arrived Packets");

                last = sw.Elapsed.TotalMilliseconds;
                FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
                EntityManager.Update();
                FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Adding/Removing Entities (Add:{EntityManager.EntitiesToAdd.Count}, Remove: {EntityManager.EntitiesToRemove.Count})");


                while (fixedUpdateAcc >= fixedUpdateTime)
                {
                    FixedUpdate(fixedUpdateTime, sw);
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                }

                last = sw.Elapsed.TotalMilliseconds;
                FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
                await OutgoingPacketQueue.SendAll();
                FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Sending Packets");

                var tickTime = sw.Elapsed.TotalMilliseconds;
                FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds:0.000}] [{tickTime:0.000}]Frame Finished - Sleeping for {fixedUpdateTime * 1000 - tickTime}");
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, fixedUpdateTime * 1000 - tickTime)));
                TicksPerSecond++;
            }
        }
        private static void FixedUpdate(float dt, Stopwatch sw)
        {
            var last = sw.Elapsed.TotalMilliseconds;
            FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
            Collections.Grid.Clear();
            FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Clearing Grid");

            last = sw.Elapsed.TotalMilliseconds;
            FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
            foreach (var kvp in EntityManager.Entities)
            {
                var entity = kvp.Value;

                LifetimeSystem.Update(dt, entity);
                MoveSystem.Update(dt, entity);
                RotationSystem.Update(dt, entity);
                HealthSystem.Update(dt, entity);

                Collections.Grid.Insert(entity);
            }

            FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Running Systems & Populating Grid");

            last = sw.Elapsed.TotalMilliseconds;
            FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
            CollisionDetection.Process();
            FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] Collisions");

            
            last = sw.Elapsed.TotalMilliseconds;
            FConsole.Write($"[{sw.Elapsed.TotalMilliseconds:0.000}] ");
            for (int i = 0; i < TimedThings.Length; i++)
                TimedThings[i].Update(dt);
            FConsole.WriteLine($"[{sw.Elapsed.TotalMilliseconds - last:0.00}ms] TimedThings");
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in EntityManager.Players)
                kvp.Value.Send(packet);
        }
    }
}