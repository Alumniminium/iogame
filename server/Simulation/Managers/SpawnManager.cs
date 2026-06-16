using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Managers;

public static class SpawnManager
{
    public static readonly Dictionary<int, int> MapResources = [];
    private static readonly HashSet<RectangleF> SafeZones = [];
    private const int HORIZONTAL_EDGE_SPAWN_OFFSET = 5; // Don't spawn #for N pixels from the edges
    private const int VERTICAL_EDGE_SPAWN_OFFSET = 5; // Don't spawn for N pixels from the edges

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

        // Drops use sensor mode to detect players without physical collision
        uint pickupCategory = (uint)CollisionCategory.Pickup;
        uint pickupMask = (uint)CollisionCategory.Player; // Only detect players
        var bodyId = PhysicsWorld.CreateBody(position, 0f, false, shapeType, resource.Density, 0.3f, resource.Elasticity, pickupCategory, pickupMask, 0, true, true);
        var box2DBody = new PhysicsComponent(bodyId, false, resource.Color, resource.Density, resource.Sides);
        box2DBody.SetLinearVelocity(vel);

        var ltc = new LifeTimeComponent(lifeTime);
        var pickable = new PickableTagComponent();

        ntt.Set(ref box2DBody);
        ntt.Set(ref ltc);
        ntt.Set(ref pickable);
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
        // For filled structures, we create multiple 1x1 entities arranged in the desired pattern
        var entities = new List<NTT>();

        if (shapeType == ShapeType.Circle)
        {
            // Create filled circle using 1x1 entities
            CreateFilledCircle(width / 2f, position, color, entities);
        }
        else
        {
            // Create filled rectangle using 1x1 entities
            CreateFilledRectangle(width, height, position, rotationDeg, color, entities);
        }

        // Return the first entity as the primary structure reference
        // (In practice, you might want to return a different reference or collection)
        return ref entities.Count > 0 ? ref NttWorld.GetEntity(entities[0].Id) : ref NttWorld.CreateEntity();
    }

    private static void CreateFilledRectangle(int width, int height, Vector2 center, float rotationDeg, uint color, List<NTT> entities)
    {
        var rotationRad = rotationDeg * MathF.PI / 180f;
        var cos = MathF.Cos(rotationRad);
        var sin = MathF.Sin(rotationRad);

        // Calculate starting position (top-left corner)
        var startX = -(width - 1) / 2f;
        var startY = -(height - 1) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate local position relative to center
                var localX = startX + x;
                var localY = startY + y;

                // Apply rotation
                var rotatedX = localX * cos - localY * sin;
                var rotatedY = localX * sin + localY * cos;

                // Calculate world position
                var worldPos = new Vector2(center.X + rotatedX, center.Y + rotatedY);

                // Create 1x1 entity at this position
                var ntt = NttWorld.CreateEntity();
                var bodyId = PhysicsWorld.CreateBody(worldPos, rotationRad, true, ShapeType.Box, 1.0f, 0.5f, 0.1f);
                var box2DBody = new PhysicsComponent(bodyId, true, color, 1.0f, 4);

                ntt.Set(ref box2DBody);
                entities.Add(ntt);
            }
        }
    }

    private static void CreateFilledCircle(float radius, Vector2 center, uint color, List<NTT> entities)
    {
        var diameter = (int)(radius * 2);
        var radiusSquared = radius * radius;

        for (int y = 0; y <= diameter; y++)
        {
            for (int x = 0; x <= diameter; x++)
            {
                // Calculate distance from center
                var localX = x - radius;
                var localY = y - radius;
                var distanceSquared = localX * localX + localY * localY;

                // Only create entity if it's within the circle
                if (distanceSquared <= radiusSquared)
                {
                    var worldPos = new Vector2(center.X + localX, center.Y + localY);

                    // Create 1x1 entity at this position
                    var ntt = NttWorld.CreateEntity();
                    var bodyId = PhysicsWorld.CreateBody(worldPos, 0f, true, ShapeType.Circle, 1.0f, 0.5f, 0.1f);
                    var box2DBody = new PhysicsComponent(bodyId, true, color, 1.0f, 0);

                    ntt.Set(ref box2DBody);
                    entities.Add(ntt);
                }
            }
        }
    }

    public static NTT Spawn(in BaseResource resource, Vector2 position, Vector2 velocity)
    {
        var ntt = NttWorld.CreateEntity();

        var hlt = new HealthComponent(resource.Health, resource.Health);

        var rotation = (float)Random.Shared.NextDouble() * MathF.PI * 2;
        var shapeType = resource.Sides switch
        {
            4 => ShapeType.Box,
            3 => ShapeType.Triangle,
            _ => ShapeType.Circle
        };

        var bodyId = PhysicsWorld.CreateBody(position, rotation, false, shapeType, resource.Density, 0.3f, resource.Elasticity);
        var box2DBody = new PhysicsComponent(bodyId, false, resource.Color, resource.Density, resource.Sides);
        box2DBody.SetLinearVelocity(velocity);

        var amount = 5;
        var pik = new DropResourceComponent(amount);

        ntt.Set(ref box2DBody);
        ntt.Set(ref hlt);
        ntt.Set(ref pik);
        return ntt;
    }

    public static void CreateSpawner(int x, int y, int unitId, TimeSpan interval, int minPopulation, int maxPopulation, uint color)
    {
        var ntt = NttWorld.CreateEntity();
        var position = new Vector2(x, y);
        var spwn = new SpawnerComponent(unitId, interval, 1, maxPopulation, minPopulation);
        var bodyId = PhysicsWorld.CreateCircleBody(position, true, 1.0f, 0.5f, 0.1f);
        var box2DBody = new PhysicsComponent(bodyId, true, color, 1.0f, 0);

        ntt.Set(ref box2DBody);
        ntt.Set(ref spwn);
    }
    public static void SpawnBullets(in NTT owner, ref Vector2 position, ref WeaponComponent wep, ref Vector2 velocity, uint color)
    {
        var ntt = NttWorld.CreateEntity();

        var bul = new BulletComponent(owner);
        var bulletMass = 0.1f;

        // Bullets use same negative group as owner - they won't collide with each other
        int bulletGroup = -(Math.Abs(owner.Id.GetHashCode()) % 1000 + 1); // Ensure negative
        uint bulletCategory = (uint)CollisionCategory.Bullet;
        uint bulletMask = (uint)CollisionCategory.All;

        var bodyId = PhysicsWorld.CreateCircleBody(position, false, bulletMass, 0.1f, 0.8f, bulletCategory, bulletMask, bulletGroup);
        var box2DBody = new PhysicsComponent(bodyId, false, color, bulletMass, 0);
        box2DBody.SetLinearVelocity(velocity);

        var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));
        var bdc = new BodyDamageComponent(wep.BulletDamage);

        ntt.Set(ref box2DBody);
        ntt.Set(ref bul);
        ntt.Set(ref ltc);
        ntt.Set(ref bdc);
    }
    public static Vector2 GetRandomDirection()
    {
        var x = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
        var y = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
        return new Vector2(x, y);
    }
    public static Vector2 PlayerSpawnPoint => new(50, Game.MapSize.Y - 50); // Near the spawners

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