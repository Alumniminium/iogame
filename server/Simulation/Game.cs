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
        public static readonly Vector2 MapSize = new(1_500, 1_500);
        public static readonly Grid Grid = new((int)MapSize.X, (int)MapSize.Y, 20, 20);
        public const int TargetTps = 60;
        public static uint CurrentTick { get; private set; }
        private const string SLEEP = "Sleep";
        private const string WORLD_UPDATE = "World.Update";

        static Game()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            PixelWorld.Systems.Add(new SpawnSystem());
            PixelWorld.Systems.Add(new LifetimeSystem());
            PixelWorld.Systems.Add(new ViewportSystem());
            PixelWorld.Systems.Add(new BoidSystem());
            PixelWorld.Systems.Add(new InputSystem());
            PixelWorld.Systems.Add(new EnergySystem());
            PixelWorld.Systems.Add(new ShieldSystem());
            PixelWorld.Systems.Add(new WeaponSystem());
            PixelWorld.Systems.Add(new EngineSystem());
            PixelWorld.Systems.Add(new PhysicsSystem());
            PixelWorld.Systems.Add(new CollisionDetector());
            PixelWorld.Systems.Add(new PickupCollisionResolver());
            PixelWorld.Systems.Add(new BodyDamageResolver());
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

            Db.LoadBaseResources();

            // SpawnManager.CreateSpawner((int)(MapSize.X / 2.5f), 20, 3, TimeSpan.FromMilliseconds(250), 1, 200, Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateSpawner((int)(MapSize.X / 3), 20, 4, TimeSpan.FromMilliseconds(250), 1, 200, Convert.ToUInt32("80ED99", 16));

            // SpawnManager.CreateSpawner((int)(MapSize.X / 1.25f), 20, 5, TimeSpan.FromMilliseconds(250), 1, 200, Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateSpawner((int)(MapSize.X / 1.125f), 20, 6, TimeSpan.FromMilliseconds(250), 1, 200, Convert.ToUInt32("80ED99", 16));
            SpawnManager.Respawn();

            // SpawnManager.CreateStructure(500,5, new Vector2(600, 850), 20f,Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateStructure(500,5, new Vector2(1200, 850), -20f, Convert.ToUInt32("10EFAA", 16));
            // SpawnManager.CreateStructure(50,5, new Vector2(820, 900), 0f,Convert.ToUInt32("434343", 16));
            // SpawnManager.CreateStructure(50,5, new Vector2(980, 900), 0f,Convert.ToUInt32("30ED99", 16));

            // SpawnManager.CreateStructure(500,5, new Vector2(600, 850), 0f,Convert.ToUInt32("80ED99", 16));
            // SpawnManager.CreateStructure(500,5, new Vector2(1200, 850), 0f, Convert.ToUInt32("10EFAA", 16));
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
                    for (var i = 0; i < PixelWorld.Systems.Count; i++)
                    {
                        var system = PixelWorld.Systems[i];
                        last = sw.Elapsed.TotalMilliseconds;
                        system.Update(fixedUpdateTime);
                        PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - last);

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
                        }
                        FConsole.WriteLine(lines);

                        onSecond = 0;
                    }
                    last = sw.Elapsed.TotalMilliseconds;
                    PixelWorld.Update(true);
                    PerformanceMetrics.AddSample(WORLD_UPDATE, sw.Elapsed.TotalMilliseconds - last);

                    fixedUpdateAcc -= fixedUpdateTime;
                    CurrentTick++;
                    PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
                }
                await OutgoingPacketQueue.SendAll().ConfigureAwait(false);
                var tickTime = sw.Elapsed.TotalMilliseconds;
                last = sw.Elapsed.TotalMilliseconds;
                var sleepTime = (int)Math.Max(0, fixedUpdateTime * 1000 - tickTime);
                Thread.Sleep(sleepTime);
                PerformanceMetrics.AddSample(SLEEP, sw.Elapsed.TotalMilliseconds - last);
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