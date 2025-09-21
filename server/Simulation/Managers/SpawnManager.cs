using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Managers;

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

    internal static NTT SpawnDrop(in BaseResource resource, Vector2 position, in TimeSpan lifeTime, Vector2 vel)
    {
        ref var ntt = ref NttWorld.CreateEntity();

        var shapeType = resource.Sides switch
        {
            4 => ShapeType.Box,
            3 => ShapeType.Triangle,
            _ => ShapeType.Circle
        };

        var bodyId = Box2DPhysicsWorld.CreateBody(position, 0f, false, shapeType, resource.Size, resource.Size, resource.Mass, 0.3f, resource.Elasticity);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, resource.Color, shapeType, resource.Size, resource.Size, resource.Size / 2f, resource.Mass, resource.Sides);
        box2DBody.SetLinearVelocity(vel);
        box2DBody.SyncFromBox2D();

        var syn = new NetSyncComponent(ntt, SyncThings.Position);
        var ltc = new LifeTimeComponent(ntt, lifeTime);

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
        ntt.Set(ref ltc);
        return ntt;
    }

    public static void Respawn()
    {
        foreach (var (id, baseResource) in Db.BaseResources)
        {
            MapResources.TryAdd(id, 0);

            for (var i = MapResources[id]; i < baseResource.MaxAliveNum; i++)
            {
                var spawnPoint = GetRandomSpawnPoint();
                // Spawn(in baseResource, spawnPoint, Vector2.Zero);
                MapResources[id]++;
            }
        }
    }

    internal static ref NTT CreateStructure(int width, int height, Vector2 position, float rotationDeg, uint color, ShapeType shapeType)
    {
        ref var ntt = ref NttWorld.CreateEntity();

        var rotationRad = rotationDeg * MathF.PI / 180f;
        var bodyId = Box2DPhysicsWorld.CreateBody(position, rotationRad, true, shapeType, width, height, 1.0f, 0.5f, 0.1f);
        var radius = shapeType == ShapeType.Circle ? width / 2f : 0.5f;
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, true, color, shapeType, width, height, radius, 1.0f);
        box2DBody.SyncFromBox2D();
        var syn = new NetSyncComponent(ntt, SyncThings.Position | SyncThings.Shield);

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
        return ref ntt;
    }

    public static NTT Spawn(in BaseResource resource, Vector2 position, Vector2 velocity)
    {
        var ntt = NttWorld.CreateEntity();

        var hlt = new HealthComponent(ntt, resource.Health, resource.Health);

        var rotation = (float)Random.Shared.NextDouble() * MathF.PI * 2;
        var shapeType = resource.Sides switch
        {
            4 => ShapeType.Box,
            3 => ShapeType.Triangle,
            _ => ShapeType.Circle
        };

        var bodyId = Box2DPhysicsWorld.CreateBody(position, rotation, false, shapeType, resource.Size, resource.Size, resource.Mass, 0.3f, resource.Elasticity);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, resource.Color, shapeType, resource.Size, resource.Size, resource.Size / 2f, resource.Mass, resource.Sides);
        box2DBody.SetLinearVelocity(velocity);
        box2DBody.SyncFromBox2D();

        var syn = new NetSyncComponent(ntt, SyncThings.Position | SyncThings.Health);
        var amount = 5;
        var pik = new DropResourceComponent(ntt, amount);

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
        ntt.Set(ref hlt);
        ntt.Set(ref pik);
        return ntt;
    }

    public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation, uint color)
    {
        var ntt = NttWorld.CreateEntity();
        var position = new Vector2(x, y);
        var spwn = new SpawnerComponent(ntt, unitId, interval, 1, maxPopulation, minPopulation);
        var bodyId = Box2DPhysicsWorld.CreateCircleBody(position, 10f, true, 1.0f, 0.5f, 0.1f);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, true, color, ShapeType.Circle, 20f, 20f, 10f, 1.0f);
        box2DBody.SyncFromBox2D();
        var syn = new NetSyncComponent(ntt, SyncThings.Position);

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
        ntt.Set(ref spwn);
    }
    public static void SpawnBullets(in NTT owner, ref Vector2 position, ref WeaponComponent wep, ref Vector2 velocity, uint color)
    {
        var ntt = NttWorld.CreateEntity();

        var bul = new BulletComponent(owner);
        var bulletMass = 0.1f;
        var bodyId = Box2DPhysicsWorld.CreateCircleBody(position, wep.BulletSize, false, bulletMass, 0.1f, 0.8f);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, color, ShapeType.Circle, wep.BulletSize * 2f, wep.BulletSize * 2f, wep.BulletSize, bulletMass);
        box2DBody.SetLinearVelocity(velocity);
        box2DBody.SyncFromBox2D();

        var ltc = new LifeTimeComponent(ntt, TimeSpan.FromSeconds(5));
        var syn = new NetSyncComponent(ntt, SyncThings.Position);
        var bdc = new BodyDamageComponent(ntt, wep.BulletDamage);

        ntt.Set(ref box2DBody);
        ntt.Set(ref bul);
        ntt.Set(ref ltc);
        ntt.Set(ref syn);
        ntt.Set(ref bdc);
    }
    public static Vector2 GetRandomDirection()
    {
        var x = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
        var y = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
        return new Vector2(x, y);
    }
    public static Vector2 PlayerSpawnPoint => new(500, Game.MapSize.Y - 500); // Near the spawners

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
        var spawnPoint = new Vector2(x, y);
        return spawnPoint;
    }
}