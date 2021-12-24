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
        private const int HORIZONTAL_EDGE_SPAWN_OFFSET = 50; // Don't spawn #for N pixels from the edges
        private const int VERTICAL_EDGE_SPAWN_OFFSET = 150; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            SafeZones.Add(new RectangleF(0, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // Player Base left edge
            SafeZones.Add(new RectangleF(Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // enemy base right edge
            SafeZones.Add(new RectangleF(0, 0, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));                                        // Top edge
            SafeZones.Add(new RectangleF(0, Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));  // Bottom edge
        }

        public static void Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<ShapeEntity>();
            var entity = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var pos = new PositionComponent(position);
            var shp = new ShapeComponent(resource.Sides, resource.Size, resource.Color);
            var hlt = new HealthComponent(resource.Health, resource.Health, 0);
            var phy = new PhysicsComponent(resource.Mass, resource.Elasticity, resource.Drag);
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.Position | SyncThings.Health);
            
            phy.Velocity = velocity;
            entity.Rect = new RectangleF(position.X - shp.Radius, position.Y - shp.Radius, shp.Size, shp.Size);

            entity.Entity.Add(ref syn);
            entity.Entity.Add(ref vwp);
            entity.Entity.Add(ref pos);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref phy);
            PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);

            lock (Game.Tree)
                Game.Tree.Add(entity);
        }

        public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation, int spawnRadius)
        {
            var id = IdGenerator.Get<Structure>();
            var entity = new Structure
            {
                Entity = PixelWorld.CreateEntity(id)
            };
            var position = new Vector2(x, y);
            var pos = new PositionComponent(position);
            var spwn = new SpawnerComponent(unitId, interval, 1, maxPopulation, minPopulation);
            var shp = new ShapeComponent(8, 5, 0);
            var vwp = new ViewportComponent(shp.Size);
            var hlt = new HealthComponent(10000, 10000, 100);
            var phy = new PhysicsComponent(float.MaxValue, 0, 1);
            var syn = new NetSyncComponent(SyncThings.Health);

            entity.Entity.Add(ref syn);

            entity.Entity.Add(ref phy);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref vwp);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref spwn);
            entity.Entity.Add(ref pos);

            PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);

            lock (Game.Tree)
                Game.Tree.Add(entity);
        }

        public static void AddShapeTo(in PixelEntity entity, int size, int sides)
        {
            var shp = new ShapeComponent(sides, size, 0);
            var vwp = new ViewportComponent(shp.Size);

            entity.Add(ref shp);
            entity.Add(ref vwp);

            if (!PixelWorld.HasAttachedShapeEntity(in entity))
            {
                var shpEntity = new ShapeEntity
                {
                    Entity = entity
                };
                PixelWorld.AttachEntityToShapeEntity(in entity, shpEntity);
                lock (Game.Tree)
                    Game.Tree.Add(shpEntity);
            }
        }

        public static void SpawnBullets(ref PixelEntity owner, ref Vector2 position, ref Vector2 velocity)
        {
            var id = IdGenerator.Get<Bullet>();
            var entity = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var bul = new BulletComponent(owner);
            var pos = new PositionComponent(position);
            var shp = new ShapeComponent(1, 5, Convert.ToUInt32("00bbf9", 16));
            var hlt = new HealthComponent(5, 5, 0);
            var phy = new PhysicsComponent((float)Math.Pow(5, 3), 0.01f);
            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.Position);

            entity.Entity.Add(ref syn);
            phy.Velocity = velocity;

            entity.Entity.Add(ref vwp);
            entity.Entity.Add(ref bul);
            entity.Entity.Add(ref pos);
            entity.Entity.Add(ref shp);
            entity.Entity.Add(ref hlt);
            entity.Entity.Add(ref phy);
            entity.Entity.Add(ref ltc);
            PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);
            entity.Rect = new RectangleF(Math.Clamp(position.X - shp.Size, shp.Size, Game.MapSize.X - shp.Size), Math.Clamp(position.Y - shp.Size, shp.Size, Game.MapSize.Y - shp.Size), shp.Size, shp.Size);

            lock (Game.Tree)
                Game.Tree.Add(entity);
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

                var boi = new BoidComponent((byte)Random.Shared.Next(0, 4));
                var hlt = new HealthComponent(100, 100, 1);
                var eng = new EngineComponent(100);
                var pos = new PositionComponent(GetRandomSpawnPoint());
                var inp = new InputComponent(Vector2.Normalize(GetRandomVelocity()), Vector2.Zero, false, 0);
                var vwp = new ViewportComponent(250);
                var shp = new ShapeComponent(3 + boi.Flock, 3, Convert.ToUInt32("00bbf9", 16));
                var phy = new PhysicsComponent((float)Math.Pow(shp.Size, 3), 1, 0.01f);
                var syn = new NetSyncComponent(SyncThings.Health | SyncThings.Position);

                entity.Entity.Add(ref syn);
                entity.Rect = new RectangleF(pos.Position.X - shp.Radius, pos.Position.Y - shp.Radius, shp.Size, shp.Size);

                entity.Entity.Add(ref boi);
                entity.Entity.Add(ref pos);
                entity.Entity.Add(ref vwp);
                entity.Entity.Add(ref shp);
                entity.Entity.Add(ref hlt);
                entity.Entity.Add(ref phy);
                entity.Entity.Add(ref eng);
                entity.Entity.Add(ref inp);
                PixelWorld.AttachEntityToShapeEntity(in entity.Entity, entity);

                lock (Game.Tree)
                    Game.Tree.Add(entity);
            }
        }

        public static Vector2 GetRandomVelocity()
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