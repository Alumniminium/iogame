using System.Drawing;
using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation.Managers
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

        public static T Spawn<T>(Vector2 position) where T : ShapeEntity, new()
        {
            var id = IdGenerator.Get<T>();
            var entity = new T
            {
                Entity = World.CreateEntity(id)
            };
            World.AttachEntityToShapeEntity(entity.Entity,entity);
            var pos = new PositionComponent(position);
            entity.Entity.Add(pos);
            CollisionDetection.Grid.Insert(entity);
            return entity;
        }
        public static ShapeEntity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<BaseResource>();
            var entity = new ShapeEntity
            {
                Entity = World.CreateEntity(id)
            };

            var pos = new PositionComponent(position);
            var shp = new ShapeComponent((byte)resource.Sides,(ushort)resource.Size);
            var hlt = new HealthComponent(resource.Health,resource.Health,1);
            entity.BodyDamage = resource.BodyDamage;
            var phy = new PhysicsComponent(resource.Mass,resource.Elasticity, resource.Drag);
            var vel = new VelocityComponent(velocity.X,velocity.Y);
            var spd = new SpeedComponent((uint)resource.MaxSpeed);
            
            entity.Entity.Add(pos);
            entity.Entity.Add(shp);
            entity.Entity.Add(hlt);
            entity.Entity.Add(phy);
            entity.Entity.Add(vel);
            entity.Entity.Add(spd);
            World.AttachEntityToShapeEntity(entity.Entity,entity);
            CollisionDetection.Grid.Insert(entity);
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