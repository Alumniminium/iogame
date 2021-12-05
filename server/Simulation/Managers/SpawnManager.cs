using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Simulation.Systems;
using iogame.Util;

namespace iogame.Simulation.Managers
{
    public static class SpawnManager
    {
        static readonly List<Rectangle> SafeZones = new();
        static readonly Dictionary<int, int> MapResources = new();

        public const int HorizontalEdgeSpawnOffset = 4; // Don't spawn #for N pixels from the edges
        public const int VerticalEdgeSpawnOffset = 4; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            foreach (var baseResource in Db.BaseResources)
                MapResources.Add(baseResource.Key, 0);

            SafeZones.Add(new Rectangle(0, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // Player Base left edge
            SafeZones.Add(new Rectangle(Game.MAP_WIDTH - HorizontalEdgeSpawnOffset, 0, HorizontalEdgeSpawnOffset, Game.MAP_HEIGHT)); // enemy base right edge
            SafeZones.Add(new Rectangle(0, 0, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));                                        // Top edge
            SafeZones.Add(new Rectangle(0, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset, Game.MAP_WIDTH, VerticalEdgeSpawnOffset));  // Bottom edge
        }

        public static ShapeEntity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<ShapeEntity>();
            var entity = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };
            PixelWorld.AttachEntityToShapeEntity(entity.Entity, entity);
            ref var pos = ref entity.Entity.Add<PositionComponent>();
            ref var vel = ref entity.Entity.Add<VelocityComponent>();
            ref var spd = ref entity.Entity.Add<SpeedComponent>();
            ref var shp = ref entity.Entity.Add<ShapeComponent>();
            ref var hlt = ref entity.Entity.Add<HealthComponent>();
            ref var phy = ref entity.Entity.Add<PhysicsComponent>();
            ref var dmg = ref entity.Entity.Add<DamageComponent>();
            ref var vwp = ref entity.Entity.Add<ViewportComponent>();
            ref var col = ref entity.Entity.Add<ColliderComponent>();

            pos.Position = position;
            shp.Sides = (byte)resource.Sides;
            shp.Size = (ushort)resource.Size;
            shp.Color = resource.Color;
            shp.BorderColor = resource.BorderColor;
            hlt.Health = resource.Health;
            hlt.MaxHealth = resource.Health;
            hlt.HealthRegenFactor = 1;
            phy.Mass = resource.Mass;
            phy.Elasticity = resource.Elasticity;
            phy.Drag = resource.Drag;
            vel.Velocity = velocity;
            spd.Speed = (uint)resource.MaxSpeed;
            dmg.Damage = resource.BodyDamage;
            vwp.ViewDistance = shp.Size;
            vwp.EntitiesVisible = Array.Empty<ColliderComponent>();
            vwp.EntitiesVisibleLastSync = Array.Empty<ColliderComponent>();
            col.Rect = new RectangleF(position.X - shp.Radius, position.Y - shp.Radius, shp.Radius, shp.Radius);
            col.EntityId = id;
            Game.Tree.Add(col);
            return entity;
        }
        public static ShapeEntity SpawnBullets(Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<Bullet>();
            var bullet = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            PixelWorld.AttachEntityToShapeEntity(bullet.Entity, bullet);

            ref var pos = ref bullet.Entity.Add<PositionComponent>();
            ref var vel = ref bullet.Entity.Add<VelocityComponent>();
            ref var vwp = ref bullet.Entity.Add<ViewportComponent>();
            ref var spd = ref bullet.Entity.Add<SpeedComponent>();
            ref var shp = ref bullet.Entity.Add<ShapeComponent>();
            ref var hlt = ref bullet.Entity.Add<HealthComponent>();
            ref var phy = ref bullet.Entity.Add<PhysicsComponent>();
            ref var lfc = ref bullet.Entity.Add<LifeTimeComponent>();
            ref var col = ref bullet.Entity.Add<ColliderComponent>();

            pos.Position = position;
            vwp.ViewDistance = 10;
            vwp.EntitiesVisible = Array.Empty<ColliderComponent>();
            vwp.EntitiesVisibleLastSync = Array.Empty<ColliderComponent>();
            shp.Sides = 32;
            shp.Size = 10;
            shp.Color = Convert.ToUInt32("00bbf9", 16);
            hlt.Health = 100;
            hlt.MaxHealth = 100;
            hlt.HealthRegenFactor = 0;
            phy.Mass = (float)Math.Pow(shp.Size, 3);
            phy.Elasticity = 0;
            phy.Drag = 0f;
            spd.Speed = 125;
            lfc.LifeTimeSeconds = 10;
            vel.Velocity = velocity;
            col.Rect = new RectangleF(position.X - shp.Radius, position.Y - shp.Radius, shp.Radius, shp.Radius);
            col.EntityId = id;
            Game.Tree.Add(col);
            return bullet;
        }
        public static void SpawnBoids(int num = 100)
        {
            for (int i = 0; i < num; i++)
            {
                var id = IdGenerator.Get<Boid>();
                var boid = new ShapeEntity
                {
                    Entity = PixelWorld.CreateEntity(id)
                };

                PixelWorld.AttachEntityToShapeEntity(boid.Entity, boid);

                ref var pos = ref boid.Entity.Add<PositionComponent>();
                ref var boi = ref boid.Entity.Add<BoidComponent>();
                ref var inp = ref boid.Entity.Add<InputComponent>();

                ref var vel = ref boid.Entity.Add<VelocityComponent>();
                ref var vwp = ref boid.Entity.Add<ViewportComponent>();
                ref var spd = ref boid.Entity.Add<SpeedComponent>();
                ref var shp = ref boid.Entity.Add<ShapeComponent>();
                ref var hlt = ref boid.Entity.Add<HealthComponent>();
                ref var phy = ref boid.Entity.Add<PhysicsComponent>();
                ref var col = ref boid.Entity.Add<ColliderComponent>();
                // ref var dmg = ref boid.Entity.Add<DamageComponent>();

                pos.Position = GetRandomSpawnPoint();
                boi.Flock = 0;
                vwp.ViewDistance = 50;
                vwp.EntitiesVisible = Array.Empty<ColliderComponent>();
                vwp.EntitiesVisibleLastSync = Array.Empty<ColliderComponent>();
                shp.Sides = 3;
                shp.Size = 5;
                shp.Color = Convert.ToUInt32("00bbf9", 16);
                hlt.Health = 100;
                hlt.MaxHealth = 100;
                hlt.HealthRegenFactor = 1;
                phy.Mass = (float)Math.Pow(shp.Size, 3);
                phy.Elasticity = 0;
                phy.Drag = 0.02f;
                spd.Speed = 25;
                // dmg.Damage = 1;
                inp.MovementAxis = Vector2.Normalize(GetRandomVelocity());
                col.Rect = new RectangleF(pos.Position.X - shp.Radius, pos.Position.Y - shp.Radius, shp.Radius, shp.Radius);
                col.EntityId = id;
                Game.Tree.Add(col);
            }
        }
        public static void Respawn()
        {
            foreach (var kvp in MapResources)
            {
                var max = Db.BaseResources[kvp.Key].MaxAliveNum;
                for (int i = kvp.Value; i < max; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = Vector2.Normalize(GetRandomVelocity());
                    Spawn(Db.BaseResources[kvp.Key], spawnPoint, velocity);
                    MapResources[kvp.Key]++;
                }
            }
        }
        public static Vector2 GetRandomVelocity()
        {
            var x = Random.Shared.Next(-1500, 1500);
            var y = Random.Shared.Next(-1500, 1500);
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint() => new(Random.Shared.Next(2, HorizontalEdgeSpawnOffset), Random.Shared.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset));

        public static Vector2 GetRandomSpawnPoint()
        {
            bool valid = false;
            int x = 0;
            int y = 0;

            while (!valid)
            {
                x = Random.Shared.Next(HorizontalEdgeSpawnOffset, Game.MAP_WIDTH - HorizontalEdgeSpawnOffset);
                y = Random.Shared.Next(VerticalEdgeSpawnOffset, Game.MAP_HEIGHT - VerticalEdgeSpawnOffset);

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