using System.Drawing;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public static class SpawnManager
    {
        public static List<Rectangle> SafeZones = new();
        public static Dictionary<int, int> MapResources =new();

        public const int HorizontalEdgeSpawnOffset = 6000; // Don't spawn #for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 3000; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            foreach(var baseResource in Db.BaseResources)
                MapResources.Add(baseResource.Key, 0);
            
            SafeZones.Add(new Rectangle(0, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // Player Base left edge
            SafeZones.Add(new Rectangle(Game.MAP_WIDTH - HorizontalEdgeSpawnOffset, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // enemy base right edge
            SafeZones.Add(new Rectangle(0, 0, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));                                        // Top edge
            SafeZones.Add(new Rectangle(0, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));  // Bottom edge
        }

        public static T Spawn<T>(Vector2 position) where T : Entity, new()
        {
            var id = IdGenerator.Get<T>();
            var entity = new T
            {
                UniqueId = id,
                PositionComponent = new PositionComponent(position)
            };

            EntityManager.AddEntity(entity);
            return entity;
        }
        public static Entity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<BaseResource>();
            var entity = new Entity
            {
                UniqueId = id,
                PositionComponent = new PositionComponent(position),
                ShapeComponent = new ShapeComponent((byte)resource.Sides,(ushort)resource.Size),
                HealthComponent = new HealthComponent(resource.Health,resource.Health,1),
                BodyDamage = resource.BodyDamage,
                PhysicsComponent = new PhysicsComponent(resource.Mass,resource.Elasticity, resource.Drag),
                VelocityComponent = new VelocityComponent(velocity.X,velocity.Y,(uint)resource.MaxSpeed)
            };

            EntityManager.AddEntity(entity);
            return entity;
        }

        public static void Respawn()
        {
            foreach(var kvp in MapResources)
            {
                var max = Db.BaseResources[kvp.Key].MaxAliveNum;
                for(int i = kvp.Value; i < max; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = GetRandomVelocity();
                    Spawn(Db.BaseResources[kvp.Key], spawnPoint, velocity);
                    MapResources[kvp.Key]++;
                }
            }
        }
        public static Vector2 GetRandomVelocity()
        {
            var x = Game.Random.Next(-1500, 1500);
            var y = Game.Random.Next(-1500, 1500);
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint() => new(Game.Random.Next(500, HorizontalEdgeSpawnOffset), Game.Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset));

        public static Vector2 GetRandomSpawnPoint()
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