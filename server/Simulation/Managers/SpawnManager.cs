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
        public static readonly Dictionary<int,int> MapResources = new ();
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

        public static void Respawn()
        {
            foreach(var (id,baseResource) in Db.BaseResources)
            {
                var max = baseResource.MaxAliveNum;
                MapResources.TryAdd(id, 0);

                for (var i = MapResources[id]; i < max; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = Vector2.Normalize(GetRandomVelocity());
                    Spawn(baseResource, spawnPoint, velocity);
                    MapResources[id]++;
                    
                    // if(i%100000== 0)
                        PixelWorld.Update();
                }
            }
        }

        public static void Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<ShapeEntity>();
            var ntt = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var shp = new ShapeComponent(resource.Sides, resource.Size, resource.Color);
            var hlt = new HealthComponent(resource.Health, resource.Health, 0);
            var phy = new PhysicsComponent(position,resource.Mass, resource.Elasticity, resource.Drag);
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.Position | SyncThings.Health);
            
            phy.Velocity = velocity;
            ntt.Rect = new RectangleF(position.X - shp.Radius, position.Y - shp.Radius, shp.Size, shp.Size);

            ntt.Entity.Set(ref syn);
            ntt.Entity.Set(ref vwp);
            ntt.Entity.Set(ref shp);
            ntt.Entity.Set(ref hlt);
            ntt.Entity.Set(ref phy);
            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            // lock (Game.Tree)
                Game.Tree.Add(ntt);
            MapResources[shp.Sides]++;
        }

        public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation, int spawnRadius)
        {
            var id = IdGenerator.Get<Structure>();
            var ntt = new Structure
            {
                Entity = PixelWorld.CreateEntity(id)
            };
            var position = new Vector2(x, y);
            var spwn = new SpawnerComponent(unitId, interval, 1, maxPopulation, minPopulation);
            var shp = new ShapeComponent(8, 50, 0);
            var vwp = new ViewportComponent(shp.Size);
            var hlt = new HealthComponent(10000, 10000, 100);
            var phy = new PhysicsComponent(position,float.MaxValue, 0, 1);
            var syn = new NetSyncComponent(SyncThings.Health);

            ntt.Entity.Set(ref syn);
            ntt.Entity.Set(ref phy);
            ntt.Entity.Set(ref hlt);
            ntt.Entity.Set(ref vwp);
            ntt.Entity.Set(ref shp);
            ntt.Entity.Set(ref spwn);

            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            lock (Game.Tree)
                Game.Tree.Add(ntt);
        }

        public static void AddShapeTo(in PixelEntity ntt, int size, int sides)
        {
            var shp = new ShapeComponent(sides, size, 0);
            var vwp = new ViewportComponent(shp.Size);

            ntt.Set(ref shp);
            ntt.Set(ref vwp);
            var shpEntity = new ShapeEntity { Entity = ntt };
            PixelWorld.AttachEntityToShapeEntity(in ntt, shpEntity);
            
            lock (Game.Tree)
                Game.Tree.Add(shpEntity);
        }

        public static void SpawnBullets(in PixelEntity owner, ref Vector2 position, ref Vector2 velocity)
        {
            var id = IdGenerator.Get<Bullet>();
            var shpNtt = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var bul = new BulletComponent(in owner);
            var shp = new ShapeComponent(1, 5, Convert.ToUInt32("00bbf9", 16));
            var hlt = new HealthComponent(5, 5, 0);
            var phy = new PhysicsComponent(position,(float)Math.Pow(5, 3), 0.01f);
            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.Position);

            shpNtt.Entity.Set(ref syn);
            phy.Velocity = velocity;

            shpNtt.Entity.Set(ref vwp);
            shpNtt.Entity.Set(ref bul);
            shpNtt.Entity.Set(ref shp);
            shpNtt.Entity.Set(ref hlt);
            shpNtt.Entity.Set(ref phy);
            shpNtt.Entity.Set(ref ltc);
            PixelWorld.AttachEntityToShapeEntity(in shpNtt.Entity, shpNtt);
            shpNtt.Rect = new RectangleF(Math.Clamp(position.X - shp.Size, shp.Size, Game.MapSize.X - shp.Size), Math.Clamp(position.Y - shp.Size, shp.Size, Game.MapSize.Y - shp.Size), shp.Size, shp.Size);
            Game.Tree.Add(shpNtt);
        }
        public static void SpawnBoids(int num = 100)
        {
            for (var i = 0; i < num; i++)
            {
                var id = IdGenerator.Get<Boid>();
                var ntt = new ShapeEntity
                {
                    Entity = PixelWorld.CreateEntity(id)
                };

                var boi = new BoidComponent((byte)Random.Shared.Next(0, 4));
                var hlt = new HealthComponent(100, 100, 1);
                var eng = new EngineComponent(100);
                var inp = new InputComponent(Vector2.Normalize(GetRandomVelocity()), Vector2.Zero);
                var vwp = new ViewportComponent(250);
                var shp = new ShapeComponent(3 + boi.Flock, 3, Convert.ToUInt32("00bbf9", 16));
                var phy = new PhysicsComponent(GetRandomSpawnPoint(),(float)Math.Pow(shp.Size, 3), 1, 0.01f);
                var syn = new NetSyncComponent(SyncThings.Health | SyncThings.Position);

                ntt.Entity.Set(ref syn);
                ntt.Rect = new RectangleF(phy.Position.X - shp.Radius, phy.Position.Y - shp.Radius, shp.Size, shp.Size);

                ntt.Entity.Set(ref boi);
                ntt.Entity.Set(ref vwp);
                ntt.Entity.Set(ref shp);
                ntt.Entity.Set(ref hlt);
                ntt.Entity.Set(ref phy);
                ntt.Entity.Set(ref eng);
                ntt.Entity.Set(ref inp);
                PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

                lock (Game.Tree)
                    Game.Tree.Add(ntt);
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