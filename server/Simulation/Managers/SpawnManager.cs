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
        private const int VERTICAL_EDGE_SPAWN_OFFSET = 50; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            SafeZones.Add(new RectangleF(0, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // Player Base left edge
            SafeZones.Add(new RectangleF(Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // enemy base right edge
            SafeZones.Add(new RectangleF(0, 0, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));                                        // Top edge
            SafeZones.Add(new RectangleF(0, Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));  // Bottom edge
        }
        
        public static void SpawnDrops(Vector2 dropperPos, int amount, BaseResource dropper)
        {
            var size = (int)Math.Max(1, dropper.Size / (amount * 0.5));
            
            for(var i = 0; i < amount; i++)
                {
                    var lifetime = TimeSpan.FromSeconds(Random.Shared.Next(3,11));
                    var position = dropperPos + (GetRandomDirection() * 2);
                    SpawnDrop(Db.BaseResources[Random.Shared.Next(3,dropper.Sides)], position, size,dropper.Color, lifetime, GetRandomDirection() * 100);
                }
                for(var i = 0; i < amount; i++)
                {
                    var lifetime = TimeSpan.FromSeconds(Random.Shared.Next(3,11));
                    var position = dropperPos + (GetRandomDirection() * 2);
                    SpawnDrop(Db.BaseResources[Random.Shared.Next(3,dropper.Sides)], position, size/2,dropper.Color, lifetime, GetRandomDirection() * 100);
                }
        }

        public static void SpawnPolygon(Vector2 pos)
        {
            var id = IdGenerator.Get<Asteroid>();
            var ntt = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };
            var pol = new PolygonComponent();
			pol.Points.Add(new Vector2(0, 50));
			pol.Points.Add(new Vector2(50,0));
			pol.Points.Add(new Vector2(150,80));
			pol.Points.Add(new Vector2(160,200));
			pol.Points.Add(new Vector2(-10, 190));
			pol.Offset(pos);
            var phy = new PhysicsComponent(pos, 1, 0.2f, 0.01f);
            var syn = new NetSyncComponent(SyncThings.All);

            ntt.Entity.Add(ref syn);
            ntt.Entity.Add(ref phy);
            ntt.Entity.Add(ref pol);
            
            var center = pol.Center();
            ntt.Rect = new RectangleF(center.X,center.Y,200,200);

            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            lock (Game.Tree)
                Game.Tree.Add(ntt);
        }

        internal static ShapeEntity SpawnDrop(BaseResource resource,Vector2 position, int size, uint color, TimeSpan lifeTime, Vector2 vel)
        {            
            var id = IdGenerator.Get<Drop>();
            var ntt = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var shp = new ShapeComponent(resource.Sides, size,color);
            var phy = new PhysicsComponent(position,resource.Mass, resource.Elasticity, resource.Drag);
            var syn = new NetSyncComponent(SyncThings.All);
            var ltc  = new LifeTimeComponent(lifeTime);
            
            phy.Velocity = vel;
            ntt.Rect = new Rectangle((int)position.X - (int)shp.Radius, (int)position.Y - (int)shp.Radius, (int)shp.Size, (int)shp.Size);

            ntt.Entity.Add(ref syn);
            ntt.Entity.Add(ref shp);
            ntt.Entity.Add(ref phy);
            ntt.Entity.Add(ref ltc);

            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            lock (Game.Tree)
                Game.Tree.Add(ntt);

            MapResources[shp.Sides]++;
            return ntt;
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
                    var velocity = GetRandomDirection();
                    Spawn(baseResource, spawnPoint, velocity);
                    MapResources[id]++;
                    
                    // if(i%100000== 0)
                        PixelWorld.Update(false);
                }
            }
        }

        public static ShapeEntity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var id = IdGenerator.Get<ShapeEntity>();
            var ntt = new ShapeEntity
            {
                Entity = PixelWorld.CreateEntity(id)
            };

            var shp = new ShapeComponent(resource.Sides, resource.Size, resource.Color);
            var hlt = new HealthComponent(resource.Health, resource.Health, 0);
            var phy = new PhysicsComponent(position,resource.Mass, resource.Elasticity, resource.Drag);
            var syn = new NetSyncComponent(SyncThings.All);

            // if ( Random.Shared.Next(0,100) > 50)
            // {
                var amount = 5;
                var pik = new DropResourceComponent(shp.Sides, amount);
                ntt.Entity.Add(ref pik);
            // }
            
            phy.Velocity = velocity;
            ntt.Rect = new Rectangle((int)position.X - (int)shp.Radius, (int)position.Y - (int)shp.Radius, (int)shp.Size, (int)shp.Size);

            ntt.Entity.Add(ref syn);
            ntt.Entity.Add(ref shp);
            ntt.Entity.Add(ref hlt);
            ntt.Entity.Add(ref phy);
            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            lock (Game.Tree)
                Game.Tree.Add(ntt);
            MapResources[shp.Sides]++;
            return ntt;
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
            var syn = new NetSyncComponent(SyncThings.All);
            ntt.Entity.Add(ref syn);
            ntt.Entity.Add(ref phy);
            ntt.Entity.Add(ref hlt);
            ntt.Entity.Add(ref vwp);
            ntt.Entity.Add(ref shp);
            ntt.Entity.Add(ref spwn);

            PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

            lock (Game.Tree)
                Game.Tree.Add(ntt);
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
            var phy = new PhysicsComponent(position,MathF.Pow(5, 3), 0.5f,0f);
            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.All);
            // var dmg = new DamageComponent(owner.Id,10);

            // shpNtt.Entity.Add(ref dmg);
            shpNtt.Entity.Add(ref syn);
            phy.Velocity = velocity;

            shpNtt.Entity.Add(ref vwp);
            shpNtt.Entity.Add(ref bul);
            shpNtt.Entity.Add(ref shp);
            shpNtt.Entity.Add(ref hlt);
            shpNtt.Entity.Add(ref phy);
            shpNtt.Entity.Add(ref ltc);
            PixelWorld.AttachEntityToShapeEntity(in shpNtt.Entity, shpNtt);
            shpNtt.Rect = new Rectangle((int)(position.X - shp.Radius), (int)(position.Y - shp.Radius), shp.Size, shp.Size);
            
                lock (Game.Tree)
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
                var inp = new InputComponent(GetRandomDirection(), Vector2.Zero);
                var vwp = new ViewportComponent(250);
                var shp = new ShapeComponent(3 + boi.Flock, 3, Convert.ToUInt32("00bbf9", 16));
                var phy = new PhysicsComponent(GetRandomSpawnPoint(),MathF.Pow(shp.Size, 3), 1, 0.01f);
                var syn = new NetSyncComponent(SyncThings.All);

                ntt.Entity.Add(ref syn);
                ntt.Rect = new Rectangle((int)phy.Position.X - (int)shp.Radius, (int)phy.Position.Y - (int)shp.Radius, shp.Size, shp.Size);

                ntt.Entity.Add(ref boi);
                ntt.Entity.Add(ref vwp);
                ntt.Entity.Add(ref shp);
                ntt.Entity.Add(ref hlt);
                ntt.Entity.Add(ref phy);
                ntt.Entity.Add(ref eng);
                ntt.Entity.Add(ref inp);
                PixelWorld.AttachEntityToShapeEntity(in ntt.Entity, ntt);

                lock (Game.Tree)
                    Game.Tree.Add(ntt);
            }
        }

        public static Vector2 GetRandomDirection()
        {
            var x = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            var y = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint() => new(Random.Shared.Next(HORIZONTAL_EDGE_SPAWN_OFFSET, (int)Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET), Random.Shared.Next((int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET*3, (int)Game.MapSize.Y-VERTICAL_EDGE_SPAWN_OFFSET));

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