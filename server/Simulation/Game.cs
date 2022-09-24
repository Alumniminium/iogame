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

namespace server.Simulation
{
    public static class Game
    {
        public static readonly Vector2 MapSize = new(2_000, 5_000);
        public static readonly Grid Grid = new((int)MapSize.X, (int)MapSize.Y, 10, 10);
        public const int TargetTps = 60;
        public static uint CurrentTick { get; private set; }
        private const string SLEEP = "Sleep";
        private const string WORLD_UPDATE = "World.Update";

        static Game()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            PerformanceMetrics.RegisterSystem(WORLD_UPDATE);
            PerformanceMetrics.RegisterSystem(SLEEP);
            PerformanceMetrics.RegisterSystem(nameof(Game));

            Db.LoadBaseResources();
            SpawnManager.Respawn();
            var worker = new Thread(GameLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };

            FastNoiseLite noise = new();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            for (int i = 0; i < MapSize.X; i += 2)
            {
                var y = MapSize.Y + noise.GetNoise(i * 0.01f, i * 0.01f) * 500;
                if (y > MapSize.Y)
                    SpawnManager.CreateStructure(2, 2, new Vector2(i, MapSize.Y), 0f, Convert.ToUInt32("30ED99", 16));
                else
                    SpawnManager.CreateStructure(2, 2, new Vector2(i, y), 45f, Convert.ToUInt32("30ED99", 16));
            }

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