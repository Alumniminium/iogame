using System.Drawing;
using System.Numerics;
using iogame.Simulation.Entities;

namespace iogame.Simulation
{
    public class SpawnManager
    {
        public static Random Random = new Random();
        public const int HorizontalEdgeSpawnOffset = 6000; // Don't spawn for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 3000; // Don't spawn for N pixels from the edges

        public const int YellowSquaresMax = 10000;
        public const int RedTrianglesMax = 1000;
        public const int PurplePentagonsMax = 100;

        public List<Rectangle> SafeZones = new List<Rectangle>();

        public int YellowSquaresAlive = 0;
        public int RedTrianglesAlive = 0;

        internal static Vector2 GetPlayerSpawnPoint() => new Vector2(Random.Next(250, HorizontalEdgeSpawnOffset), Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset));

        public int PurplePentagonsAlive = 0;

        public SpawnManager()
        {
            SafeZones.Add(new Rectangle(0, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // Player Base left edge
            SafeZones.Add(new Rectangle(Game.MAP_WIDTH - HorizontalEdgeSpawnOffset, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // enemy base right edge
            SafeZones.Add(new Rectangle(0, 0, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));                                        // Top edge
            SafeZones.Add(new Rectangle(0, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));  // Bottom edge
        }

        public void Spawn()
        {
            while (YellowSquaresAlive < YellowSquaresMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = new YellowSquare(spawnPoint.X, spawnPoint.Y, velocity.X, velocity.Y);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
                YellowSquaresAlive++;
            }
            while (RedTrianglesAlive < RedTrianglesMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = new RedTriangle(spawnPoint.X, spawnPoint.Y, velocity.X, velocity.Y);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
                RedTrianglesAlive++;
            }
            while (PurplePentagonsAlive < PurplePentagonsMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = new PurplePentagon(spawnPoint.X, spawnPoint.Y, velocity.X, velocity.Y);
                entity.UniqueId = (uint)Collections.Entities.Count;
                Collections.Entities.TryAdd(entity.UniqueId, entity);
                Collections.Grid.Insert(entity);
                PurplePentagonsAlive++;
            }
            Collections.EntitiesArray = Collections.Entities.Values.ToArray();
        }
        public Vector2 GetRandomVelocity()
        {
            var x = Random.Next(-1500, 1500);
            var y = Random.Next(-1500, 1500);
            return new Vector2(x, y);
        }
        public Vector2 GetRandomSpawnPoint()
        {
            bool valid = false;
            int x = 0;
            int y = 0;

            while (!valid)
            {
                x = Random.Next(HorizontalEdgeSpawnOffset, Game.MAP_WIDTH - HorizontalEdgeSpawnOffset);
                y = Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset);

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
}