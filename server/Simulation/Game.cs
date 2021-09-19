using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public static class Collections
    {
        public static ConcurrentDictionary<uint, Player> Players = new();
        public static ConcurrentDictionary<uint, Entity> Entities = new();
    }
    public class Game
    {
        public static uint TickCounter;
        public const int MAP_WIDTH = 3500;
        public const int MAP_HEIGHT = 1500;
        public const float DRAG = 0.9f;
        private Thread worker;
        public void Start()
        {
            var random = new Random();
            for (uint i = 0; i < 200; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new YellowSquare(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 50; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new RedTriangle(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 25; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new PurplePentagon(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
            }
            for (uint i = 0; i < 1; i++)
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
            var stopwatch = new Stopwatch();
            var fps = 1000;
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

                foreach (var kvp in Collections.Entities)
                {
                    kvp.Value.Update(dt);

                    if (totalTime >= 0.1)
                    {
                        kvp.Value.Screen.Send(MovementPacket.Create(kvp.Key, kvp.Value.Look, kvp.Value.Position, kvp.Value.Velocity),true);
                    }
                }

                if (totalTime >= 0.1)
                {
                    TickCounter++;
                    totalTime = 0;
                    Console.WriteLine(curFps);
                }
                CheckCollisions(dt);
                var tickTIme = stopwatch.ElapsedMilliseconds;

                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme))); //Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(1, 16)));
            }
        }

        private void CheckCollisions(float dt)
        {                        
            foreach (var kvp in Collections.Entities)
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
                entity.InCollision = false;
            }

            foreach (var a in Collections.Entities)
            {
                foreach (var b in a.Value.Screen.Entities)
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

                        var impulse = 2 * speed / (Math.Pow(a.Value.Size, 3) + Math.Pow(b.Value.Size, 3));
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