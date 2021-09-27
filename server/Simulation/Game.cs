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
        public const int MAP_WIDTH = 100000;
        public const int MAP_HEIGHT = 40000;
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
        }
        internal void RemoveEntity(Entity entity)
        {
            Collections.Players.TryRemove(entity.UniqueId, out _);
            Collections.Entities.TryRemove(entity.UniqueId, out _);
            Collections.Grid.Remove(entity);
        }

        public void GameLoop()
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

                Update(now, dt);

                var tickTIme = stopwatch.ElapsedMilliseconds;

                if(targetTps != 1000)
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
            }
        }

        private void Update(DateTime now, float dt)
        {
            MovedThisTick.Clear();
            foreach (var kvp in Collections.Entities)
            {
                var curPos = kvp.Value.Position;
                kvp.Value.Update(dt);
                if (curPos != kvp.Value.Position)
                    {
                        Collections.Grid.Move(curPos, kvp.Value);
                        MovedThisTick.Add(kvp.Value);
                    }
            }
            CheckCollisions();

            if (lastSync.AddMilliseconds(33) <= now)
            {
                lastSync = now;
                TickCounter++;

                foreach (var pkvp in Collections.Players)
                {
                    pkvp.Value.Send(PingPacket.Create(DateTime.UtcNow.Ticks, 0));
                    var vectorC = new Vector2(((int)pkvp.Value.Position.X) / Grid.W, ((int)pkvp.Value.Position.Y) / Grid.H);
                    var entityLists = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value);
                    pkvp.Value.Screen.Check(entityLists);
                    foreach (var list in entityLists)
                        foreach (var entity in list)
                            pkvp.Value.Send(MovementPacket.Create(entity.UniqueId, entity.Look, entity.Position, entity.Velocity));
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
            foreach (var a in MovedThisTick)
            {
                a.InCollision = false;
                var visible = Collections.Grid.GetEntitiesSameCell(a);
                foreach (var b in visible)
                {
                    if (a.UniqueId == b.UniqueId || b.InCollision)
                        continue;

                    if (a.CheckCollision(b))
                    {
                        a.InCollision = true;
                        b.InCollision = true;
                        var collision = Vector2.Subtract(b.Position, a.Position);
                        var distance = Vector2.Distance(b.Position, a.Position);
                        var collisionNormalized = collision / distance;
                        var relativeVelocity = Vector2.Subtract(a.Velocity, b.Velocity);
                        var speed = Vector2.Dot(relativeVelocity, collisionNormalized);

                        if (speed < 0)
                            continue;

                        var impulse = 2 * speed / (a.Mass + b.Mass);
                        var fa = new Vector2((float)(impulse * b.Mass * collisionNormalized.X), (float)(impulse * b.Mass * collisionNormalized.Y));
                        var fb = new Vector2((float)(impulse * a.Mass * collisionNormalized.X), (float)(impulse * a.Mass * collisionNormalized.Y));

                        a.Velocity -= fa;
                        b.Velocity += fb;

                        a.Health--;
                        b.Health--;
                    }
                }
                var entity = a;

                if (entity.Position.X < entity.Radius)
                {
                    entity.Velocity.X = Math.Abs(entity.Velocity.X) * DRAG;
                    entity.Position.X = entity.Radius;
                }
                else if (entity.Position.X > MAP_WIDTH - entity.Size)
                {
                    entity.Velocity.X = -Math.Abs(entity.Velocity.X) * DRAG;
                    entity.Position.X = MAP_WIDTH - entity.Size;
                }

                if (entity.Position.Y < entity.Radius)
                {
                    entity.Velocity.Y = Math.Abs(entity.Velocity.Y) * DRAG;
                    entity.Position.Y = entity.Radius;
                }
                else if (entity.Position.Y > MAP_HEIGHT - entity.Size)
                {
                    entity.Velocity.Y = -Math.Abs(entity.Velocity.Y) * DRAG;
                    entity.Position.Y = MAP_HEIGHT - entity.Size;
                }


                if (a.Health <= 0)
                    RemoveEntity(a);
            }
        }
    }
}