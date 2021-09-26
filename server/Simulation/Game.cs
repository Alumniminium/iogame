using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class Collision
    {
        public Entity A;
        public Entity B;

        public Collision(Entity a, Entity b)
        {
            A = a;
            B = b;
        }
    }
    public class SpawnManager
    {
        public static Random Random = new Random();
        public const int HorizontalEdgeSpawnOffset = 20000; // Don't spawn for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 2000; // Don't spawn for N pixels from the edges

        public const int YellowSquaresMax = 10000;
        public const int RedTrianglesMax = 6000;
        public const int PurplePentagonsMax = 1000;

        public List<Rectangle> SafeZones = new List<Rectangle>();

        public int YellowSquaresAlive = 0;
        public int RedTrianglesAlive = 0;
        public int PurplePentagonsAlive = 0;

        public void Spawn()
        {

        }
        public Vector2 GetRandomSpawnPoint()
        {
            bool valid = false;
            int x = 0;
            int y = 0;

            while (!valid)
            {
                x = Random.Next(HorizontalEdgeSpawnOffset, Game.MAP_WIDTH - HorizontalEdgeSpawnOffset);
                y = Random.Next(HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT - HorizontalEdgeSpawnOffset);

                valid = true;
                foreach (var rect in SafeZones)
                {
                    if (rect.Contains(x, y))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                    break;

            }
            return new Vector2(x, y);
        }
    }
    public class Game
    {
        public static Random random = new Random();
        public static uint TickCounter;
        public const int MAP_WIDTH = 300000;
        public const int MAP_HEIGHT = 100000;
        public const float DRAG = 0.9997f;
        private Thread worker;


        public DateTime lastSync = DateTime.UtcNow;
        public DateTime lastTpsCheck = DateTime.UtcNow;
        public uint tpsCounter = 0;

        public void Start()
        {
            for (uint i = 0; i < 5000; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new YellowSquare(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
            }
            for (uint i = 0; i < 1000; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new RedTriangle(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
            }
            for (uint i = 0; i < 250; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new PurplePentagon(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
            }
            for (uint i = 0; i < 10; i++)
            {
                var x = random.Next(0, MAP_WIDTH);
                var y = random.Next(0, MAP_HEIGHT);
                var vX = random.Next(-10, 11);
                var vY = random.Next(-10, 11);
                var entity = new PurpleOctagon(x, y, vX, vY);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
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
            Collections.Grid.Insert(player);
        }
        internal void RemovePlayer(Player player)
        {
            Collections.Players.TryRemove(player.UniqueId, out _);
            Collections.Entities.TryRemove(player.UniqueId, out _);
            Collections.Grid.Insert(player);
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
                Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(0, sleepTime - tickTIme)));
            }
        }

        private void Update(DateTime now, float dt)
        {
            foreach (var kvp in Collections.Entities)
            {
                var curPos = kvp.Value.Position;
                kvp.Value.Update(dt);
                if (curPos != kvp.Value.Position)
                    Collections.Grid.Move(curPos, kvp.Value);
            }
            CheckCollisions();

            if (lastSync.AddMilliseconds(50) <= now)
            {
                lastSync = now;
                TickCounter++;

                foreach (var pkvp in Collections.Players)
                {
                    pkvp.Value.Send(PingPacket.Create(DateTime.UtcNow.Ticks, 0));
                    var vectorC = new Vector2(((int)pkvp.Value.Position.X) / Grid.W, ((int)pkvp.Value.Position.Y) / Grid.H);
                    var entityLists = Collections.Grid.GetEntitiesSameAndSurroundingCells(pkvp.Value).ToArray();
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
            foreach (var a in Collections.Entities)
                a.Value.InCollision = false;

            foreach (var a in Collections.Entities)
            {
                var visible = Collections.Grid.GetEntitiesSameCell(a.Value);
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

                        if (speed < 0)
                            continue;

                        var impulse = 2 * speed / (a.Value.Mass + b.Mass);
                        var fa = new Vector2((float)(impulse * b.Mass * collisionNormalized.X), (float)(impulse * b.Mass * collisionNormalized.Y));
                        var fb = new Vector2((float)(impulse * a.Value.Mass * collisionNormalized.X), (float)(impulse * a.Value.Mass * collisionNormalized.Y));

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
            }
        }
    }
}