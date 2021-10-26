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
            var stopwatch = new Stopwatch();
            var sleepTime = 1000 / TARGET_TPS;
            var fixedUpdateAcc = 0f;
            var fixedUpdateTime = 1f / PHYSICS_TPS;


            while (true)
            {
                var dt = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                fixedUpdateAcc += dt;

                IncomingPacketQueue.ProcessAll();
                EntityManager.Update();

                if (fixedUpdateAcc >= fixedUpdateTime)
                {
                    FixedUpdate(fixedUpdateTime);
                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                }
                await OutgoingPacketQueue.SendAll();

                var tickTIme = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
                TicksPerSecond++;
            }
        }
        private static void FixedUpdate(float dt)
        {
            Collections.Grid.Clear();
            foreach (var kvp in EntityManager.Entities)
            {
                var entity = kvp.Value;

                LifetimeSystem.Update(dt, entity);
                MoveSystem.Update(dt, entity);
                RotationSystem.Update(dt, entity);
                HealthSystem.Update(dt, entity);

                Collections.Grid.Insert(entity);
            }

            CollisionDetection.Process();

            for (int i = 0; i < TimedThings.Length; i++)
                TimedThings[i].Update(dt);
        }
        public static void Broadcast(byte[] packet)
        {
            foreach (var kvp in EntityManager.Players)
                kvp.Value.Send(packet);
        }
    }
}