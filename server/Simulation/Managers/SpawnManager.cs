using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Managers
{
    public static class SpawnManager
    {
        public static readonly Dictionary<int, int> MapResources = new();
        private static readonly HashSet<RectangleF> SafeZones = new();
        private const int HORIZONTAL_EDGE_SPAWN_OFFSET = 150; // Don't spawn #for N pixels from the edges
        private const int VERTICAL_EDGE_SPAWN_OFFSET = 150; // Don't spawn for N pixels from the edges

        static SpawnManager()
        {
            SafeZones.Add(new RectangleF(0, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // Player Base left edge
            SafeZones.Add(new RectangleF(Game.MapSize.X - HORIZONTAL_EDGE_SPAWN_OFFSET, 0, HORIZONTAL_EDGE_SPAWN_OFFSET, Game.MapSize.Y)); // enemy base right edge
            SafeZones.Add(new RectangleF(0, 0, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));                                        // Top edge
            SafeZones.Add(new RectangleF(0, Game.MapSize.Y - VERTICAL_EDGE_SPAWN_OFFSET, Game.MapSize.X, VERTICAL_EDGE_SPAWN_OFFSET));  // Bottom edge
        }

        internal static PixelEntity SpawnDrop(in BaseResource resource, Vector2 position, int size, uint color, in TimeSpan lifeTime, Vector2 vel)
        {
            ref var ntt = ref PixelWorld.CreateEntity(EntityType.Pickable);

            var phy = PhysicsComponent.CreateCircleBody(ntt.Id, size / 2, position, 1, 1f, color);
            var syn = new NetSyncComponent(ntt.Id, SyncThings.Position);
            var ltc = new LifeTimeComponent(ntt.Id, lifeTime);
            var aabb = new AABBComponent(ntt.Id, new RectangleF(position.X - size / 2, position.Y - size / 2, size, size));

            phy.LinearVelocity = vel;

            ntt.Add(ref syn);
            ntt.Add(ref phy);
            ntt.Add(ref ltc);
            ntt.Add(ref aabb);
            Game.Grid.Add(in ntt, ref phy);

            return ntt;
        }

        public static void Respawn()
        {
            foreach (var (id, baseResource) in Db.BaseResources)
            {
                MapResources.TryAdd(id, 0);

                for (var i = MapResources[id]; i < baseResource.MaxAliveNum; i++)
                //for (var i = MapResources[id]; i < 1; i++)
                {
                    var spawnPoint = GetRandomSpawnPoint();
                    var velocity = Vector2.Zero;//GetRandomDirection();
                    Spawn(in baseResource, spawnPoint, velocity);
                    MapResources[id]++;
                }
            }
        }

        internal static ref PixelEntity CreateStructure(int width, int height, Vector2 position, float rotationDeg, uint color, ShapeType shapeType)
        {
            ref var ntt = ref PixelWorld.CreateEntity(EntityType.Static);

            var phy = shapeType == ShapeType.Box ? PhysicsComponent.CreateBoxBody(ntt.Id, width, height, position, 1, 0.1f, color) 
                                                 : PhysicsComponent.CreateCircleBody(ntt.Id, width, position, 1, 0.1f, color);

            phy.RotationRadians = rotationDeg.ToRadians();
            var syn = new NetSyncComponent(ntt.Id, SyncThings.Position | SyncThings.Shield);
            var aabb = new AABBComponent(ntt.Id, new RectangleF(position.X - width/2, position.Y-height/2, width, height));
            ntt.Add(ref syn);
            ntt.Add(ref phy);
            ntt.Add(ref aabb);
            Game.Grid.Add(ntt, ref phy);
            return ref ntt;
        }

        public static PixelEntity Spawn(in BaseResource resource, Vector2 position, Vector2 velocity)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Passive);

            var hlt = new HealthComponent(ntt.Id, resource.Health, resource.Health);

            var phy = PhysicsComponent.CreateCircleBody(ntt.Id, resource.Size / 2, position, 1, resource.Elasticity, resource.Color);
            if (resource.Sides == 4)
                phy = PhysicsComponent.CreateBoxBody(ntt.Id, resource.Size, resource.Size, position, 1, resource.Elasticity, resource.Color);
            if (resource.Sides == 3)
                phy = PhysicsComponent.CreateTriangleBody(ntt.Id, resource.Size, resource.Size, position, 1, resource.Elasticity, resource.Color);

            phy.RotationRadians = (float)Random.Shared.NextDouble() * MathF.PI * 2;
            var syn = new NetSyncComponent(ntt.Id, SyncThings.Position | SyncThings.Health);
            var aabb = new AABBComponent(ntt.Id, new RectangleF(phy.Position.X-resource.Size/2, phy.Position.Y-resource.Size/2, resource.Size, resource.Size));
            var amount = 5;
            var pik = new DropResourceComponent(ntt.Id, amount);
            ntt.Add(ref pik);

            phy.LinearVelocity = velocity;
            ntt.Add(ref syn);
            ntt.Add(ref hlt);
            ntt.Add(ref phy);
            ntt.Add(ref aabb);

            // MapResources[phy.Sides]++;
            Game.Grid.Add(in ntt, ref phy);
            return ntt;
        }

        public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation, uint color)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Static);
            var position = new Vector2(x, y);
            var spwn = new SpawnerComponent(ntt.Id, unitId, interval, 1, maxPopulation, minPopulation);
            var phy = PhysicsComponent.CreateCircleBody(ntt.Id, 10, position, 1, 1f, color);
            var syn = new NetSyncComponent(ntt.Id, SyncThings.Position);
            var aabb = new AABBComponent(ntt.Id, new RectangleF(phy.Position.X - phy.Size/2, phy.Position.Y- phy.Size/2, phy.Size, phy.Size));
            ntt.Add(ref syn);
            ntt.Add(ref phy);
            ntt.Add(ref aabb);
            // ntt.Add(ref hlt);
            // ntt.Add(ref vwp);
            ntt.Add(ref spwn);

            Game.Grid.Add(in ntt, ref phy);
        }
        public static void SpawnBullets(in PixelEntity owner, ref Vector2 position, ref WeaponComponent wep, ref Vector2 velocity, uint color)
        {
            var ntt = PixelWorld.CreateEntity(EntityType.Projectile);

            var bul = new BulletComponent(in owner);
            var phy = PhysicsComponent.CreateCircleBody(ntt.Id, wep.BulletSize, position, 1, 1f, color);
            var ltc = new LifeTimeComponent(ntt.Id, TimeSpan.FromSeconds(5));
            var aabb = new AABBComponent(ntt.Id, new RectangleF(phy.Position.X- phy.Size/2, phy.Position.Y- phy.Size/2,phy.Size, phy.Size));
            var syn = new NetSyncComponent(ntt.Id, SyncThings.Position);
            var bdc = new BodyDamageComponent(ntt.Id, wep.BulletDamage);

            ntt.Add(ref syn);
            phy.LinearVelocity = velocity;

            ntt.Add(ref aabb);
            ntt.Add(ref bul);
            ntt.Add(ref phy);
            ntt.Add(ref ltc);
            ntt.Add(ref bdc);

            Game.Grid.Add(in ntt, ref phy);
        }
        public static Vector2 GetRandomDirection()
        {
            var x = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            var y = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            return new Vector2(x, y);
        }
        public static Vector2 PlayerSpawnPoint => new(Random.Shared.Next(0, 100), Random.Shared.Next((int)Game.MapSize.Y - 50, (int)Game.MapSize.Y - 5));

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