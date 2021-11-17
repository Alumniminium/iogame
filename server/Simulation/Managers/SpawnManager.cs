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
        static readonly List<Rectangle> SafeZones = new();
        static readonly Dictionary<int, int> MapResources =new();

        public const int HorizontalEdgeSpawnOffset = 300; // Don't spawn #for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 100; // Don't spawn for N pixels from the edges

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

            entity.Entity.Add<PositionComponent>();
            ref var pos = ref entity.Entity.Get<PositionComponent>();
            pos.Position = position;

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
            ref var pos = ref ComponentList<PositionComponent>.AddFor(entity.Entity.EntityId);
            ref var vel = ref ComponentList<VelocityComponent>.AddFor(entity.Entity.EntityId);
            ref var spd = ref ComponentList<SpeedComponent>.AddFor(entity.Entity.EntityId);
            ref var shp = ref ComponentList<ShapeComponent>.AddFor(entity.Entity.EntityId);
            ref var hlt = ref ComponentList<HealthComponent>.AddFor(entity.Entity.EntityId);
            ref var phy = ref ComponentList<PhysicsComponent>.AddFor(entity.Entity.EntityId);

            pos.Position = position;
            shp.Sides = (byte)resource.Sides;
            shp.Size = (ushort)resource.Size;
            hlt.Health = resource.Health;
            hlt.MaxHealth = resource.Health;
            hlt.HealthRegenFactor = 1;
            phy.Mass = resource.Mass;
            phy.Elasticity = resource.Elasticity;
            phy.Drag = resource.Drag;
            vel.Velocity = velocity;
            spd.Speed = (uint)resource.MaxSpeed;

            entity.BodyDamage = resource.BodyDamage;
            
            
            entity.Entity.Add(ref pos);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref phy);
            entity.Entity.Add(ref vel);
            entity.Entity.Add(ref spd);
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
        public static Vector2 GetPlayerSpawnPoint() => new(Game.Random.Next(20, HorizontalEdgeSpawnOffset), Game.Random.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset));

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