using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Game
    {
        public const int MAP_WIDTH = 3500;
        public const int MAP_HEIGHT = 1500;
        public const float DRAG = 0.9f;
        public ConcurrentDictionary<uint, Entity> Entities = new();
        public ConcurrentDictionary<uint, Player> Players = new();
        private Thread worker;
        public void Start()
        {
            for (uint i = 0; i < 100; i++)
            {
                var x = Random.Shared.Next(0, MAP_WIDTH);
                var y = Random.Shared.Next(0, MAP_HEIGHT);
                var vX = Random.Shared.Next(-10, 11);
                var vY = Random.Shared.Next(-10, 11);
                var entity = new YellowSquare(x, y, vX, vY);
                entity.UniqueId = (uint)Entities.Count;
                Entities.TryAdd(entity.UniqueId, entity);
            }
            // for (uint i = 0; i < 50; i++)
            // {
            //     var x = Random.Shared.Next(0, MAP_WIDTH);
            //     var y = Random.Shared.Next(0, MAP_HEIGHT);
            //     var vX = Random.Shared.Next(-10, 11);
            //     var vY = Random.Shared.Next(-10, 11);
            //     var entity = new RedTriangle(x, y, vX, vY);
            //     entity.UniqueId = (uint)Entities.Count;
            //     Entities.TryAdd(entity.UniqueId, entity);
            // }
            // for (uint i = 0; i < 25; i++)
            // {
            //     var x = Random.Shared.Next(0, MAP_WIDTH);
            //     var y = Random.Shared.Next(0, MAP_HEIGHT);
            //     var vX = Random.Shared.Next(-10, 11);
            //     var vY = Random.Shared.Next(-10, 11);
            //     var entity = new PurplePentagon(x, y, vX, vY);
            //     entity.UniqueId = (uint)Entities.Count;
            //     Entities.TryAdd(entity.UniqueId, entity);
            // }
            // for (uint i = 0; i < 1; i++)
            // {
            //     var x = Random.Shared.Next(0, MAP_WIDTH);
            //     var y = Random.Shared.Next(0, MAP_HEIGHT);
            //     var vX = Random.Shared.Next(-10, 11);
            //     var vY = Random.Shared.Next(-10, 11);
            //     var entity = new PurpleOctagon(x, y, vX, vY);
            //     entity.UniqueId = (uint)Entities.Count;
            //     Entities.TryAdd(entity.UniqueId, entity);
            // }

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

                    if (totalTime >= 0.1)
                    {
                        foreach (var player in Players)
                            player.Value.Send(MovementPacket.Create(kvp.Key, kvp.Value.Look, kvp.Value.Position, kvp.Value.Velocity));
                    }
                }

                if (totalTime >= 0.1)
                {
                    totalTime = 0;
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
            foreach (var a in Entities)
                a.Value.InCollision = false;

            foreach (var a in Entities)
            {
                foreach (var b in Entities)
                {
                    if (a.Key == b.Key || a.Value.InCollision || b.Value.InCollision)
                        continue;

                    if (a.Value.CheckCollision(b.Value))
                    {
                        a.Value.InCollision = true;
                        b.Value.InCollision = true;
                        var collision = Vector2.Subtract(b.Value.Position, a.Value.Position);
                        var distance = Vector2.Distance(b.Value.Position, a.Value.Position);
                        var collisionNormalized = collision / distance;
                        var relativeVelocity = Vector2.Subtract(a.Value.Velocity, b.Value.Velocity);
                        var speed = Vector2.Dot(relativeVelocity, collisionNormalized);

                        //speed *= 0.5;
                        if (speed < 0)
                           continue;

                        var impulse = 2 * speed / (Math.Pow(a.Value.Size,3) + Math.Pow(b.Value.Size, 3));
                        var fa = new Vector2((float)(impulse * Math.Pow(b.Value.Size, 3) * collisionNormalized.X), (float)(impulse * Math.Pow(b.Value.Size, 3) * collisionNormalized.Y));
                        var fb = new Vector2((float)(impulse * Math.Pow(a.Value.Size, 3) * collisionNormalized.X), (float)(impulse * Math.Pow(a.Value.Size, 3) * collisionNormalized.Y));

                        a.Value.Velocity -= fa;
                        b.Value.Velocity += fb;

                        if (a.Value is Player || b.Value is Player)
                        {
                            a.Value.Health--;
                            b.Value.Health--;
                        }
                    }
                }
            }
        }
    }
}