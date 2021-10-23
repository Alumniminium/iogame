using System;
using System.Drawing;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public class SpawnManager
    {
        public List<Rectangle> SafeZones = new();

        public const int HorizontalEdgeSpawnOffset = 6000; // Don't spawn for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 3000; // Don't spawn for N pixels from the edges

        public const int YellowSquaresMax = 5000;
        public const int RedTrianglesMax = 500;
        public const int PurplePentagonsMax = 50;


        public int YellowSquaresAlive = 0;
        public int RedTrianglesAlive = 0;
        public int PurplePentagonsAlive = 0;

        public SpawnManager()
        {
            SafeZones.Add(new Rectangle(0, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // Player Base left edge
            SafeZones.Add(new Rectangle(Game.MAP_WIDTH - HorizontalEdgeSpawnOffset, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // enemy base right edge
            SafeZones.Add(new Rectangle(0, 0, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));                                        // Top edge
            SafeZones.Add(new Rectangle(0, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));  // Bottom edge
        }

        internal static T Spawn<T>(Vector2 position) where T : Entity, new()
        {
            var id = IdGenerator.Get<T>();
            
            var entity = new T
            {
                UniqueId = id,
                PositionComponent = new PositionComponent(position)
            };

            Game.AddEntity(entity);
            return entity;
        }

        public void Respawn()
        {
            while (YellowSquaresAlive < YellowSquaresMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = Spawn<YellowSquare>(spawnPoint);
                entity.VelocityComponent.Movement = velocity;

                Game.AddEntity(entity);
                YellowSquaresAlive++;
            }
            while (RedTrianglesAlive < RedTrianglesMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = Spawn<RedTriangle>(spawnPoint);
                entity.VelocityComponent.Movement = velocity;
                Game.AddEntity(entity);
                RedTrianglesAlive++;
            }
            while (PurplePentagonsAlive < PurplePentagonsMax)
            {
                var spawnPoint = GetRandomSpawnPoint();
                var velocity = GetRandomVelocity();
                var entity = Spawn<PurplePentagon>(spawnPoint);
                entity.VelocityComponent.Movement = velocity;
                Game.AddEntity(entity);
                PurplePentagonsAlive++;
            }
        }
        public Vector2 GetRandomVelocity()
        {
            var x = Game.Random.Next(-1500, 1500);
            var y = Game.Random.Next(-1500, 1500);
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint() => new(Game.Random.Next(500, HorizontalEdgeSpawnOffset), Game.Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset));

        public Vector2 GetRandomSpawnPoint()
        {
            bool valid = false;
            int x = 0;
            int y = 0;

            while (!valid)
            {
                x = Game.Random.Next(HorizontalEdgeSpawnOffset, Game.MAP_WIDTH - HorizontalEdgeSpawnOffset);
                y = Game.Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset);

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