using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Threading;
using Auios.QuadTree;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;
using server.Simulation.Net;
using server.Simulation.SpaceParition;

namespace server.Simulation;

public static class Game
{
    public class MyCustomBounds : IQuadTreeObjectBounds<AABBComponent>
    {
        public float GetBottom(AABBComponent obj) => obj.AABB.Bottom;
        public float GetTop(AABBComponent obj) => obj.AABB.Top;
        public float GetLeft(AABBComponent obj) => obj.AABB.Left;
        public float GetRight(AABBComponent obj) => obj.AABB.Right;
    }
    public static readonly Vector2 MapSize = new(1500, 100_000);
    public static readonly Grid Grid = new((int)MapSize.X, (int)MapSize.Y, 25, 25);
    public static readonly QuadTree<AABBComponent> QuadTree = new(0, 0, MapSize.X, MapSize.Y, new MyCustomBounds());
    public const int TargetTps = 60;
    public const string WORLD_UPDATE = "World.Update";
    public const string SLEEP = "Sleep";
    public static uint CurrentTick { get; private set; }

    static Game()
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        PerformanceMetrics.RegisterSystem("World.Update");
        PerformanceMetrics.RegisterSystem("Sleep");
        PerformanceMetrics.RegisterSystem(nameof(Game));

        Db.BaseResources.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value.Sides}x{x.Value.Size}px"));

        // SpawnManager.CreateSpawner((int)(MapSize.X / 4.5f), (int)(MapSize.Y - 420), 3, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));
        // SpawnManager.CreateSpawner((int)(MapSize.X / 3.5), (int)(MapSize.Y - 420), 4, TimeSpan.FromMilliseconds(500), 1, 200, Convert.ToUInt32("80ED99", 16));

        // SpawnManager.CreateSpawner((int)(MapSize.X / 1.25f), (int)(MapSize.Y - 420), 5, TimeSpan.FromMilliseconds(1500), 1, 200, Convert.ToUInt32("80ED99", 16));
        // SpawnManager.CreateSpawner((int)(MapSize.X / 1.125f), (int)(MapSize.Y - 420), 6, TimeSpan.FromMilliseconds(1500), 1, 200, Convert.ToUInt32("80ED99", 16));
        SpawnManager.Respawn();

        SpawnManager.CreateStructure(500, 5, new Vector2(300, MapSize.Y - 250), 15, Convert.ToUInt32("80ED99", 16), ShapeType.Box);
        SpawnManager.CreateStructure(500, 5, new Vector2(1200, MapSize.Y - 250), -15f, Convert.ToUInt32("10EFAA", 16), ShapeType.Box);
        SpawnManager.CreateStructure(50, 5, new Vector2(520, MapSize.Y - 250), 75f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        SpawnManager.CreateStructure(50, 5, new Vector2(980, MapSize.Y - 250), 115f, Convert.ToUInt32("30ED99", 16), ShapeType.Box);

        SpawnManager.CreateStructure(25, 250, new Vector2(MapSize.X / 2, MapSize.Y - 125), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        // SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 190), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        // SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 200), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        // SpawnManager.CreateStructure(75, 5, new Vector2(MapSize.X / 2, MapSize.Y - 210), 0f, Convert.ToUInt32("434343", 16), ShapeType.Box);
        var dome = SpawnManager.CreateStructure(50, 50, new Vector2(MapSize.X / 2, MapSize.Y - 240), 0f, Convert.ToUInt32("434343", 16), ShapeType.Circle);
        var shield = new ShieldComponent(dome.Id, 100, 10000, 0, 15, 12, 10, TimeSpan.FromSeconds(4));
        var energy = new EnergyComponent(dome.Id, 100, 100, 1000);
        dome.Add(ref shield);
        dome.Add(ref energy);
        var worker = new Thread(GameLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };


        SpawnManager.CreateStructure((int)MapSize.X, 2, new Vector2(MapSize.X / 2, MapSize.Y), 0f, Convert.ToUInt32("30ED99", 16), ShapeType.Box);

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
                    // var lines = PerformanceMetrics.Draw();
                    // FConsole.WriteLine(lines);
                    foreach (var ntt in PixelWorld.Players)
                        ntt.NetSync(PingPacket.Create());

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