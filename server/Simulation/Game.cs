using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Game
    {
        public static Random random = new();
        public static SpawnManager SpawnManager = new();
        public static uint TickCounter;
        public const int MAP_WIDTH = 90000;
        public const int MAP_HEIGHT = 30000;
        public const float DRAG = 0.99997f;
        private Thread worker;


        public DateTime lastSync = DateTime.UtcNow;
        public DateTime lastTpsCheck = DateTime.UtcNow;
        public uint tpsCounter = 0;

        public List<Entity> MovedThisTick = new List<Entity>();

        public void Start()
        {
            SpawnManager.Spawn();
            worker = new Thread(GameLoop) { IsBackground = true };
            worker.Start();
        }

        internal void AddPlayer(Player player)
        {
            var id = 1_000_000 + Collections.Players.Count;
            player.UniqueId = (uint)id;
            Collections.Players.TryAdd(player.UniqueId, player);
            Collections.Entities.TryAdd(player.UniqueId, player);
            Collections.Grid.Insert(player);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }
        internal void RemoveEntity(Entity entity)
        {
            Collections.Players.TryRemove(entity.UniqueId, out _);
            Collections.Entities.TryRemove(entity.UniqueId, out _);
            Collections.Grid.Remove(entity);
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }

        public async void GameLoop()
        {
            Console.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var stopwatch = new Stopwatch();
            var targetTps = 1000;
            var sleepTime = 1000 / targetTps;
            var prevTime = DateTime.UtcNow;

            while (true)
            {
                stopwatch.Restart();
                tpsCounter++;
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                prevTime = now;
                var curTps = Math.Round(1 / dt);

                await Update(now, dt).ConfigureAwait(false);

                var tickTIme = stopwatch.ElapsedMilliseconds;

                if (targetTps != 1000)
                    Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
            }
        }

        private async Task Update(DateTime now, float dt)
        {
            Collections.Grid.Clear();
            foreach (var kvp in Collections.Entities)
            {
                kvp.Value.Update(dt);
                Collections.Grid.Insert(kvp.Value);
            }
            CheckCollisions();            

            if (lastSync.AddMilliseconds(70) <= now)
            {
                lastSync = now;
                TickCounter++;

                foreach (var pkvp in Collections.Players)
                {
                    await pkvp.Value.Send(PingPacket.Create(DateTime.UtcNow.Ticks, 0)).ConfigureAwait(false);
                    var vectorC = new Vector2(((int)pkvp.Value.Position.X) / Grid.W, ((int)pkvp.Value.Position.Y) / Grid.H);
                    var list = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value);
                    foreach (var entity in list)
                        await pkvp.Value.Send(MovementPacket.Create(entity.UniqueId, entity.Position, entity.Velocity)).ConfigureAwait(false);
                }
            }
            if (lastTpsCheck.AddSeconds(1) <= now)
            {
                lastTpsCheck = now;

                var info = GC.GetGCMemoryInfo();
                Console.WriteLine($"TPS: {tpsCounter} | Time Spent in GC: {info.PauseTimePercentage}%");
                tpsCounter = 0;
            }
        }
        private void CheckCollisions()
        {
            foreach (var kvp in Collections.Entities)
            {
                var a = kvp.Value;

                var visible = Collections.Grid.GetEntitiesSameCell(a).ToArray();

                foreach (var b in visible)
                {
                    if (a.CheckCollision(b))
                    {
                        
                        Collections.Grid.Remove(a);
                        Collections.Grid.Remove(b);
                        var dist = a.Position - b.Position;
                        var penDepth = a.Radius + b.Radius - dist.Magnitude();
                        var penRes = dist.unit() * (penDepth / (a.InverseMass + b.InverseMass));
                        a.Position += penRes * a.InverseMass;
                        b.Position += penRes * -b.InverseMass;

                        var normal = (a.Position - b.Position).unit();
                        var relVel = a.Velocity - b.Velocity;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.Elasticity, b.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (a.InverseMass + b.InverseMass);
                        var impulseVec = normal * impulse;

                        a.Velocity += impulseVec * a.InverseMass;
                        b.Velocity += impulseVec * -b.InverseMass;
                        Collections.Grid.Insert(a);
                        Collections.Grid.Insert(b);
                    }
                }
            }
        }
    }
}