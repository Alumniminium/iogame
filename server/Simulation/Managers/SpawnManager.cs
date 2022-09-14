using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Managers
{
    public static class SpawnManager
    {
        public static readonly Dictionary<int, int> MapResources = new();
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

        public static void SpawnPolygon(Vector2 pos)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Passive);
            var pol = new PolygonComponent();
            pol.Points.Add(new Vector2(0, 50));
            pol.Points.Add(new Vector2(50, 0));
            pol.Points.Add(new Vector2(150, 80));
            pol.Points.Add(new Vector2(160, 200));
            pol.Points.Add(new Vector2(-10, 190));
            pol.Offset(pos);
            var phy = new PhysicsComponent(pos, 1, 0.2f, 0.01f);
            var syn = new NetSyncComponent(SyncThings.All);

            ntt.Add(ref syn);
            ntt.Add(ref phy);
            ntt.Add(ref pol);

            lock (Game.Grid)
                Game.Grid.Add(ntt);
        }

        internal static PixelEntity SpawnDrop(BaseResource resource, Vector2 position, int size, uint color, TimeSpan lifeTime, Vector2 vel)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Pickable);

            var shp = new ShapeComponent(resource.Sides, size, color);
            var phy = new PhysicsComponent(position, resource.Mass, resource.Elasticity, resource.Drag);
            var syn = new NetSyncComponent(SyncThings.All);
            var ltc = new LifeTimeComponent(lifeTime);

            phy.Velocity = vel;

            ntt.Add(ref syn);
            ntt.Add(ref shp);
            ntt.Add(ref phy);
            ntt.Add(ref ltc);

            lock (Game.Grid)
                Game.Grid.Add(ntt);

            MapResources[shp.Sides]++;
            return ntt;
        }

        public static void Respawn()
        {
            foreach (var (id, baseResource) in Db.BaseResources)
            {
                MapResources.TryAdd(id, 0);

                // for (var i = MapResources[id]; i < baseResource.MaxAliveNum; i++)
                for (var i = MapResources[id]; i < 1; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = Vector2.Zero;//GetRandomDirection();
                    Spawn(baseResource, spawnPoint, velocity);
                    MapResources[id]++;
                    PixelWorld.Update(false);
                }
            }
        }

        public static PixelEntity Spawn(BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Passive);

            var shp = new ShapeComponent(resource.Sides, resource.Size, resource.Color);
            var hlt = new HealthComponent(resource.Health, resource.Health, 0);
            var phy = new PhysicsComponent(position, resource.Mass, resource.Elasticity, resource.Drag);
            var syn = new NetSyncComponent(SyncThings.All);
            var vwp = new ViewportComponent(resource.Size);

            // if ( Random.Shared.Next(0,100) > 50)
            // {
            var amount = 5;
            var pik = new DropResourceComponent(shp.Sides, amount);
            ntt.Add(ref pik);
            // }

            phy.Velocity = velocity;
            ntt.Add(ref syn);
            ntt.Add(ref shp);
            ntt.Add(ref hlt);
            ntt.Add(ref phy);
            ntt.Add(ref vwp);

            MapResources[shp.Sides]++;
            Game.Grid.Add(ntt);
            return ntt;
        }

        public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Static);
            var position = new Vector2(x, y);
            var spwn = new SpawnerComponent(unitId, interval, 1, maxPopulation, minPopulation);
            var shp = new ShapeComponent(8, 50, 0);
            var vwp = new ViewportComponent(shp.Size);
            var hlt = new HealthComponent(10000, 10000, 100);
            var phy = new PhysicsComponent(position, float.MaxValue, 0, 1);
            var syn = new NetSyncComponent(SyncThings.All);
            ntt.Add(ref syn);
            ntt.Add(ref phy);
            ntt.Add(ref hlt);
            ntt.Add(ref vwp);
            ntt.Add(ref shp);
            ntt.Add(ref spwn);

            Game.Grid.Add(ntt);
        }
        public static PixelEntity SpawnBullets(in PixelEntity owner, ref Vector2 position, ref Vector2 velocity)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Projectile);

            var bul = new BulletComponent(in owner);
            var shp = new ShapeComponent(1, 5, Convert.ToUInt32("00bbf9", 16));
            var hlt = new HealthComponent(5, 5, 0);
            var phy = new PhysicsComponent(position, MathF.Pow(5, 3), 0.5f, 0f);
            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
            var vwp = new ViewportComponent(shp.Size);
            var syn = new NetSyncComponent(SyncThings.All);
            // var dmg = new DamageComponent(owner.Id,10);

            // ntt.Add(ref dmg);
            ntt.Add(ref syn);
            phy.Velocity = velocity;

            ntt.Add(ref vwp);
            ntt.Add(ref bul);
            ntt.Add(ref shp);
            ntt.Add(ref hlt);
            ntt.Add(ref phy);
            ntt.Add(ref ltc);

            Game.Grid.Add(ntt);
            return ntt;
        }
        public static void SpawnBoids(int num = 100)
        {
            for (var i = 0; i < num; i++)
            {

                var ntt = PixelWorld.CreateEntity(EntityType.Npc);
                var boi = new BoidComponent((byte)Random.Shared.Next(0, 4));
                var hlt = new HealthComponent(100, 100, 1);
                var eng = new EngineComponent(100);
                var inp = new InputComponent(GetRandomDirection(), Vector2.Zero);
                var vwp = new ViewportComponent(250);
                var shp = new ShapeComponent(3 + boi.Flock, 3, Convert.ToUInt32("00bbf9", 16));
                var phy = new PhysicsComponent(GetRandomSpawnPoint(), MathF.Pow(shp.Size, 3), 1, 0.01f);
                var syn = new NetSyncComponent(SyncThings.All);

                ntt.Add(ref syn);

                ntt.Add(ref boi);
                ntt.Add(ref vwp);
                ntt.Add(ref shp);
                ntt.Add(ref hlt);
                ntt.Add(ref phy);
                ntt.Add(ref eng);
                ntt.Add(ref inp);
                Game.Grid.Add(ntt);
            }
        }

        public static Vector2 GetRandomDirection()
        {
            var x = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            var y = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            return new Vector2(x, y);
        }
        public static Vector2 GetPlayerSpawnPoint()
        {
            return new(Random.Shared.Next(HORIZONTAL_EDGE_SPAWN_OFFSET, (int)Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET), Random.Shared.Next((int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET * 3, (int)Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET));
        }

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