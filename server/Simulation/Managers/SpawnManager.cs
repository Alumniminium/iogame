using System.Drawing;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Entities;

namespace server.Simulation.Managers
{
    public static class SpawnManager
    {
        private static readonly List<RectangleF> SafeZones = new();
        private static readonly Dictionary<int, int> MapResources = new();

        private const int HORIZONTAL_EDGE_SPAWN_OFFSET = 50; // Don't spawn #for N pixels from the edges
        private const int VERTICAL_EDGE_SPAWN_OFFSET = 150; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            foreach (var baseResource in Db.BaseResources)
                MapResources.Add(baseResource.Key, 0);

            SafeZones.Add(new RectangleF(0, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // Player Base left edge
            SafeZones.Add(new RectangleF(Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // enemy base right edge
            SafeZones.Add(new RectangleF(0, 0, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));                                        // Top edge
            SafeZones.Add(new RectangleF(0, Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));  // Bottom edge
        }

        private static ShapeEntity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<ShapeEntity>();
            var entity = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };
            PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);

            var pos = new PositionComponent(in position);
            var vel = new VelocityComponent(in velocity, Vector2.Zero);
            var shp = new ShapeComponent(resource.Sides, resource.Size, resource.Color);
            var hlt = new HealthComponent(resource.Health, resource.Health, 0);
            var phy = new PhysicsComponent(resource.Mass, resource.Elasticity, resource.Drag);
            var vwp = new ViewportComponent(shp.Size);

            entity.Rect = new RectangleF(position.X - shp.Radius, position.Y - shp.Radius, shp.Size, shp.Size);

            entity.Entity.Add(ref vwp);
            entity.Entity.Add(ref pos);
            entity.Entity.Add(ref vel);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref phy);

            lock (Game.Tree)
                Game.Tree.Add(entity);
            return entity;
        }
        public static ShapeEntity SpawnBullets(ref PixelEntity owner, ref Vector2 position, ref Vector2 velocity)
        {
            var id = IdGenerator.Get<Bullet>();
            var entity = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);
            var bul = new BulletComponent(owner);
            var pos = new PositionComponent(position);
            var vel = new VelocityComponent(velocity, Vector2.Zero);
            var spd = new SpeedComponent(125);
            var shp = new ShapeComponent(32, 5, Convert.ToUInt32("00bbf9", 16));
            var hlt = new HealthComponent(5, 5, 0);
            var phy = new PhysicsComponent((float)Math.Pow(5, 3), 0, 0);
            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
            var vwp = new ViewportComponent(shp.Size);
                
            entity.Entity.Add(ref vwp);
            entity.Entity.Add(ref bul);
            entity.Entity.Add(ref pos);
            entity.Entity.Add(ref vel);
            entity.Entity.Add(ref spd);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref phy);
            entity.Entity.Add(ref ltc);

            entity.Rect = new RectangleF(Math.Clamp(position.X - shp.Size, shp.Size, Game.MapSize.X - shp.Size), Math.Clamp(position.Y - shp.Size, shp.Size, Game.MapSize.Y - shp.Size), shp.Size, shp.Size);
            lock (Game.Tree)
                Game.Tree.Add(entity);
            return entity;
        }
        public static void SpawnBoids(int num = 100)
        {
            for (var i = 0; i < num; i++)
            {
                var id = IdGenerator.Get<Boid>();
                var entity = new ShapeEntity
                {
                    Entity = PixelWorld.CreateEntity(id)
                };

                PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);

                var boi = new BoidComponent((byte)Random.Shared.Next(0, 4));
                var hlt = new HealthComponent(100, 100, 1);
                var spd = new SpeedComponent(100);
                var vel = new VelocityComponent();
                var pos = new PositionComponent(GetRandomSpawnPoint());
                var inp = new InputComponent(Vector2.Normalize(GetRandomVelocity()), Vector2.Zero, false, 0);
                var vwp = new ViewportComponent(250);
                var shp = new ShapeComponent(3 + boi.Flock, 3, Convert.ToUInt32("00bbf9", 16));
                var phy = new PhysicsComponent((float)Math.Pow(shp.Size, 3), 1, 0.01f);
                entity.Rect = new RectangleF(pos.Position.X - shp.Radius, pos.Position.Y - shp.Radius, shp.Size, shp.Size);

                entity.Entity.Add(ref boi);
                entity.Entity.Add(ref pos);
                entity.Entity.Add(ref vwp);
                entity.Entity.Add(ref shp);
                entity.Entity.Add(ref hlt);
                entity.Entity.Add(ref phy);
                entity.Entity.Add(ref spd);
                entity.Entity.Add(ref inp);
                entity.Entity.Add(ref vel);

                lock (Game.Tree)
                    Game.Tree.Add(entity);
            }
        }
        public static void Respawn()
        {
            foreach (var kvp in MapResources)
            {
                var max = Db.BaseResources[kvp.Key].MaxAliveNum;
                for (var i = kvp.Value; i < max; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = Vector2.Zero; //.Normalize(GetRandomVelocity());
                    Spawn(Db.BaseResources[kvp.Key], spawnPoint, velocity);
                    MapResources[kvp.Key]++;
                }
            }
        }

        private static Vector2 GetRandomVelocity()
        {
            var x = Random.Shared.Next(-100, 100);
            var y = Random.Shared.Next(-100, 100);
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint() => new(Random.Shared.Next(HORIZONTAL_EDGE_SPAWN_OFFSET, (int)Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET), Random.Shared.Next((int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET * 2, (int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET));

        private static Vector2 GetRandomSpawnPoint()
        {
            bool valid;
            int x;
            int y;

            while (true)
            {
                x = Random.Shared.Next(HORIZONTAL_EDGE_SPAWN_OFFSET, (int)Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET);
                y = Random.Shared.Next(VERTICAL_EDGE_SPAWN_OFFSET, (int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET);

                valid = SafeZones.All(rect => !rect.Contains(x, y));
                if (valid)
                    break;

            }
            return new Vector2(x, y);
        }
    }
}