using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Game
    {
        public const int MAP_WIDTH = 1000;
        public const int MAP_HEIGHT = 1000;
        public const float DRAG = 0.9f;
        public ConcurrentDictionary<uint, Entity> Entities = new();
        public ConcurrentDictionary<uint, Player> Players = new();
        private Thread worker;
        public void Start()
        {
            for (uint i = 0; i < 24; i++)
            {
                var x = Random.Shared.Next(0, MAP_WIDTH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-10, 11);
                var vY = Random.Shared.Next(-10, 11);
                var entity = new YellowSquare(x, y, vX, vY);
                entity.UniqueId = (uint)Entities.Count;
                Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 12; i++)
            {
                var x = Random.Shared.Next(0, MAP_WIDTH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-10, 11);
                var vY = Random.Shared.Next(-10, 11);
                var entity = new RedTriangle(x, y, vX, vY);
                entity.UniqueId = (uint)Entities.Count;
                Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 6; i++)
            {
                var x = Random.Shared.Next(0, MAP_WIDTH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-10, 11);
                var vY = Random.Shared.Next(-10, 11);
                var entity = new PurplePentagon(x, y, vX, vY);
                entity.UniqueId = (uint)Entities.Count;
                Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 1; i++)
            {
                var x = Random.Shared.Next(0, MAP_WIDTH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-10, 11);
                var vY = Random.Shared.Next(-10, 11);
                var entity = new PurpleOctagon(x, y, vX, vY);
                entity.UniqueId = (uint)Entities.Count;
                Entities.TryAdd(entity.UniqueId, entity);
            }

            worker = new Thread(GameLoop) { IsBackground = true };
            worker.Start();
        }

        internal void AddPlayer(Player player)
        {
            var id = 1_000_000 + Players.Count;
            player.UniqueId = (uint)id;
            Players.TryAdd(player.UniqueId, player);
            Entities.TryAdd(player.UniqueId, player);
        }
        internal void RemovePlayer(Player player)
        {
            Players.TryRemove(player.UniqueId, out _);
            Entities.TryRemove(player.UniqueId, out _);
        }

        public void GameLoop()
        {
            var stopwatch = new Stopwatch();
            var fps = 60f;
            var sleepTime = 1000 / fps;
            var prevTime = DateTime.UtcNow;
            int counter = 0;
            var totalTime = 0f;
            while (true)
            {
                stopwatch.Restart();
                counter++;
                var now = DateTime.UtcNow;
                var dt = (float)(now - prevTime).TotalSeconds;
                prevTime = now;
                totalTime += dt;
                var curFps = Math.Round(1 / dt);

                foreach (var kvp in Entities)
                {
                    kvp.Value.Update(dt);

                    // if(kvp.Value is Player)
                    // continue;
                    if (totalTime >= 0.033)
                    {
                        foreach (var player in Players.Values)
                            player.Send(MovementPacket.Create(kvp.Key, kvp.Value.Look, kvp.Value.Position, kvp.Value.Velocity));
                    }
                }

                if (totalTime >= 0.033)
                {
                    totalTime = 0;

                    // Console.WriteLine(curFps);
                }
                CheckCollisions(dt);
                CheckEdgeCollisions();
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(1, sleepTime - stopwatch.ElapsedMilliseconds))); //Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(1, 16)));
            }
        }

        private void CheckEdgeCollisions()
        {
            foreach (var kvp in Entities)
            {
                var entity = kvp.Value;

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
            }
        }
        private void CheckCollisions(float dt)
        {
            foreach (var a in Entities.Values)
                a.InCollision = false;

            foreach (var a in Entities.Values)
            {
                foreach (var b in Entities.Values)
                {
                    if (a == b || a.InCollision || b.InCollision)
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

                        //speed *= 0.5;
                        if (speed < 0)
                            continue;

                        var impulse = 2 * speed / (a.Size + b.Size);
                        var fa = new Vector2(impulse * b.Size * collisionNormalized.X, impulse * b.Size * collisionNormalized.Y);
                        var fb = new Vector2(impulse * a.Size * collisionNormalized.X, impulse * a.Size * collisionNormalized.Y);

                        a.Velocity -= fa;
                        b.Velocity += fb;

                        if (a is Player || b is Player)
                        {
                            a.Health--;
                            b.Health--;
                        }
                    }
                }
            }
        }
    }
}