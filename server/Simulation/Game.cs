using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using iogame.Net.Packets;
using iogame.Simulation.Entities;
using QuadTrees;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static ConcurrentDictionary<uint, Player> Players = new();
        public static ConcurrentDictionary<uint, Entity> Entities = new();
        public static QuadTreeRect<Entity> Tree = new QuadTreeRect<Entity>(0, 0, Game.MAP_WIDTH, Game.MAP_HEIGHT);
    }
    public class Grid
    {
        public const int W = 300;
        public const int H = 300;
        public Dictionary<Vector2, List<Entity>> Cells = new();

        public void Insert(Entity entity)
        {
            var vector = new Vector2(((int)entity.Position.X) / W, ((int)entity.Position.Y) / H);

            if (!Cells.TryGetValue(vector, out var cell))
                Cells.Add(vector, new List<Entity>());
            Cells[vector].Add(entity);
        }
        public void Remove(Entity entity)
        {
            var vector = new Vector2(((int)entity.Position.X) / W, ((int)entity.Position.Y) / H);
            Cells[vector].Remove(entity);
        }
        public void Clear()
        {
            foreach (var kvp in Cells)
                kvp.Value.Clear();
        }
        public IEnumerable<Entity> GetEntitiesForPlayer(Entity entity)
        {
            var vectors = new List<Vector2>();
            var vectorC = new Vector2(((int)entity.Position.X) / W, ((int)entity.Position.Y) / H);
            vectors.Add(vectorC); //28,6
            vectors.Add(vectorC + new Vector2(1, 0)); //29,6
            vectors.Add(vectorC + new Vector2(0, 1)); //28,5
            vectors.Add(vectorC + new Vector2(1, 1));
            vectors.Add(vectorC + new Vector2(-1, 0));
            vectors.Add(vectorC + new Vector2(0, -1));
            vectors.Add(vectorC + new Vector2(-1, -1));
            vectors.Add(vectorC + new Vector2(1, -1));
            vectors.Add(vectorC + new Vector2(-1, 1));

            foreach (var vector in vectors)
            {
                if (!Cells.ContainsKey(vector))
                    Cells.Add(vector, new List<Entity>());

                foreach (var e in Cells[vector])
                    yield return e;
            }
        }
        public IEnumerable<Entity> GetEntities(Entity entity)
        {
            var vectorC = new Vector2(((int)entity.Position.X) / W, ((int)entity.Position.Y) / H);

            if (!Cells.ContainsKey(vectorC))
                Cells.Add(vectorC, new List<Entity>());

            foreach (var e in Cells[vectorC])
                yield return e;
        }
    }
    public class Game
    {
        public Grid G = new Grid();
        public static uint TickCounter;
        public const int MAP_WIDTH = 30000;
        public const int MAP_HEIGHT = 5000;
        public const float DRAG = 0.9f;
        private Thread worker;
        public void Start()
        {
            var random = new Random();
            for (uint i = 0; i < 5000; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new YellowSquare(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 2500; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new RedTriangle(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 1000; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new PurplePentagon(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 100; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new PurpleOctagon(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }

            worker = new Thread(GameLoop) { IsBackground = true };
            worker.Start();
        }

        internal void AddPlayer(Player player)
        {
            var id = 1_000_000 + Collections.Players.Count;
            player.UniqueId = (uint)id;
            Collections.Players.TryAdd(player.UniqueId, player);
            Collections.Entities.TryAdd(player.UniqueId, player);
        }
        internal void RemovePlayer(Player player)
        {
            Collections.Players.TryRemove(player.UniqueId, out _);
            Collections.Entities.TryRemove(player.UniqueId, out _);
        }

        public void GameLoop()
        {
            Console.WriteLine("Vectors Hw Acceleration: " + Vector.IsHardwareAccelerated);
            var stopwatch = new Stopwatch();
            var targetTps = 1000;
            var sleepTime = 1000 / targetTps;
            var prevTime = DateTime.UtcNow;
            var tpsCounter = 0;

            var lastSync = DateTime.UtcNow;
            var lastTpsCheck = DateTime.UtcNow;

            while (true)
            {
                stopwatch.Restart();
                tpsCounter++;
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                prevTime = now;
                var curTps = Math.Round(1 / dt);

                foreach (var kvp in Collections.Entities)
                {
                    kvp.Value.Update(dt);
                    G.Insert(kvp.Value);
                }
                // Collections.Tree.AddRange(Collections.Entities.Values);
                CheckCollisions();
                // Collections.Tree.Clear();

                if (lastSync.AddMilliseconds(100) <= now)
                {
                    foreach (var pkvp in Collections.Players)
                    {
                        var vectorC = new Vector2(((int)pkvp.Value.Position.X) / Grid.W, ((int)pkvp.Value.Position.Y) / Grid.H);
                        foreach (var entity in G.GetEntitiesForPlayer(pkvp.Value))
                            pkvp.Value.Send(MovementPacket.Create(entity.UniqueId, entity.Look, entity.Position, entity.Velocity));
                    }
                    lastSync = now;
                    TickCounter++;
                }
                if (lastTpsCheck.AddSeconds(1) <= now)
                {
                    lastTpsCheck = now;
                    var info = GC.GetGCMemoryInfo();
                    Console.WriteLine($"TPS: {tpsCounter} | Time Spent in GC: {info.PauseTimePercentage}%");
                    tpsCounter = 0;
                }

                G.Clear();
                var tickTIme = stopwatch.ElapsedMilliseconds;
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
            }
        }



        private void CheckCollisions()
        {

            foreach (var a in Collections.Entities)
            {
                // var visible = Collections.Tree.GetObjects(a.Value.ViewRect);
                var visible = G.GetEntities(a.Value);
                foreach (var b in visible)
                {
                    if (a.Key == b.UniqueId || a.Value.InCollision || b.InCollision)
                        continue;

                    if (a.Value.CheckCollision(b))
                    {
                        a.Value.InCollision = true;
                        b.InCollision = true;
                        var collision = Vector2.Subtract(b.Position, a.Value.Position);
                        var distance = Vector2.Distance(b.Position, a.Value.Position);
                        var collisionNormalized = collision / distance;
                        var relativeVelocity = Vector2.Subtract(a.Value.Velocity, b.Velocity);
                        var speed = Vector2.Dot(relativeVelocity, collisionNormalized);

                        //speed *= 0.5;
                        if (speed < 0)
                            continue;

                        var overlap = a.Value.Origin - b.Origin;
                        var off = overlap.Length() - (a.Value.Radius + b.Radius);
                        var direction = Vector2.Normalize(b.Origin - a.Value.Origin);
                        a.Value.Position += direction * off;
                        b.Position -= direction * off;

                        var impulse = 2 * speed / (Math.Pow(a.Value.Size, 3) + Math.Pow(b.Size, 3));
                        var fa = new Vector2((float)(impulse * Math.Pow(b.Size, 3) * collisionNormalized.X), (float)(impulse * Math.Pow(b.Size, 3) * collisionNormalized.Y));
                        var fb = new Vector2((float)(impulse * Math.Pow(a.Value.Size, 3) * collisionNormalized.X), (float)(impulse * Math.Pow(a.Value.Size, 3) * collisionNormalized.Y));

                        a.Value.Velocity -= fa;
                        b.Velocity += fb;

                        if (a.Value is Player || b is Player)
                        {
                            a.Value.Health--;
                            b.Health--;
                        }
                    }
                }
                var entity = a.Value;

                if (entity.Position.X < entity.Size / 2)
                {
                    entity.Velocity.X = Math.Abs(entity.Velocity.X) * DRAG;
                    entity.Position.X = entity.Size / 2;
                }
                else if (entity.Position.X > MAP_WIDTH - entity.Size)
                {
                    entity.Velocity.X = -Math.Abs(entity.Velocity.X) * DRAG;
                    entity.Position.X = MAP_WIDTH - entity.Size;
                }

                if (entity.Position.Y < entity.Size / 2)
                {
                    entity.Velocity.Y = Math.Abs(entity.Velocity.Y) * DRAG;
                    entity.Position.Y = entity.Size / 2;
                }
                else if (entity.Position.Y > MAP_HEIGHT - entity.Size)
                {
                    entity.Velocity.Y = -Math.Abs(entity.Velocity.Y) * DRAG;
                    entity.Position.Y = MAP_HEIGHT - entity.Size;
                }
                entity.InCollision = false;
            }
        }
    }
}