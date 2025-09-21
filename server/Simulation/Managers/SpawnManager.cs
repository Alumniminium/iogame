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
        uint pickupCategory = (uint)server.Enums.CollisionCategory.Pickup;
        uint pickupMask = (uint)server.Enums.CollisionCategory.Player; // Only detect players
        var bodyId = Box2DPhysicsWorld.CreateBody(position, 0f, false, shapeType, resource.Density, 0.3f, resource.Elasticity, pickupCategory, pickupMask, 0, true, true);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, resource.Color, shapeType, resource.Density, resource.Sides);
        box2DBody.SetLinearVelocity(vel);

        var syn = new NetSyncComponent(ntt, SyncThings.Position);
        var ltc = new LifeTimeComponent(ntt, lifeTime);
        var pickable = new PickableTagComponent();

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
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
                var bodyId = Box2DPhysicsWorld.CreateBody(worldPos, rotationRad, true, ShapeType.Box, 1.0f, 0.5f, 0.1f);
                var box2DBody = new Box2DBodyComponent(ntt, bodyId, true, color, ShapeType.Box, 1.0f);
                var syn = new NetSyncComponent(ntt, SyncThings.Position);

                ntt.Set(ref box2DBody);
                ntt.Set(ref syn);
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
                    var bodyId = Box2DPhysicsWorld.CreateBody(worldPos, 0f, true, ShapeType.Circle, 1.0f, 0.5f, 0.1f);
                    var box2DBody = new Box2DBodyComponent(ntt, bodyId, true, color, ShapeType.Circle, 1.0f);
                    var syn = new NetSyncComponent(ntt, SyncThings.Position);

                    ntt.Set(ref box2DBody);
                    ntt.Set(ref syn);
                    entities.Add(ntt);
                }
            }
        }
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

        var bodyId = Box2DPhysicsWorld.CreateBody(position, rotation, false, shapeType, resource.Density, 0.3f, resource.Elasticity);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, resource.Color, shapeType, resource.Density, resource.Sides);
        box2DBody.SetLinearVelocity(velocity);

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
        var bodyId = Box2DPhysicsWorld.CreateCircleBody(position, true, 1.0f, 0.5f, 0.1f);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, true, color, ShapeType.Circle, 1.0f);
        var syn = new NetSyncComponent(ntt, SyncThings.Position);

        ntt.Set(ref box2DBody);
        ntt.Set(ref syn);
        ntt.Set(ref spwn);
    }
    public static void SpawnBullets(in NTT owner, ref Vector2 position, ref WeaponComponent wep, ref Vector2 velocity, uint color)
    {
        var ntt = NttWorld.CreateEntity();

        var bul = new BulletComponent(ntt, owner);
        var bulletMass = 0.1f;

        // Bullets use same negative group as owner - they won't collide with each other
        int bulletGroup = -(Math.Abs(owner.Id.GetHashCode()) % 1000 + 1); // Ensure negative
        uint bulletCategory = (uint)server.Enums.CollisionCategory.Bullet;
        uint bulletMask = (uint)server.Enums.CollisionCategory.All;

        var bodyId = Box2DPhysicsWorld.CreateCircleBody(position, false, bulletMass, 0.1f, 0.8f, bulletCategory, bulletMask, bulletGroup);
        var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, color, ShapeType.Circle, bulletMass);
        box2DBody.SetLinearVelocity(velocity);

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
    public static Vector2 PlayerSpawnPoint => new(50, Game.MapSize.Y - 50); // Near the spawners

    public static List<NTT> CreateAsteroid(Vector2 center, int radius, Vector2 hollowSize, int seed = 0)
    {
        var asteroidId = AsteroidGenerator.GetNextAsteroidId();
        var blocks = AsteroidGenerator.GenerateAsteroid(center, radius, hollowSize, seed);
        var entities = new List<NTT>();
        var blockMap = new Dictionary<Vector2Int, NTT>();

        // Phase 1: Create all block entities
        foreach (var block in blocks)
        {
            var ntt = NttWorld.CreateEntity();
            var gridPos = ToGridPos(block.Position, center);
            blockMap[gridPos] = ntt;

            // Core components every block gets
            var asteroidBlock = new AsteroidBlockComponent
            {
                AsteroidId = asteroidId,
                IsAnchor = IsNearCenter(block.Position, center, radius * 0.3f),
                HasPhysics = false
            };

            var health = new HealthComponent(ntt, block.Health, block.Health);
            var drops = new DropResourceComponent(ntt, block.DropAmount);
            // Sync both position and health for destructible blocks
            var sync = new NetSyncComponent(ntt, SyncThings.Position | SyncThings.Health);

            ntt.Set(ref asteroidBlock);
            ntt.Set(ref health);
            ntt.Set(ref drops);
            ntt.Set(ref sync);

            entities.Add(ntt);
        }

        // Phase 2: Set up neighbor relationships and edge physics
        foreach (var (gridPos, ntt) in blockMap)
        {
            var neighbors = new AsteroidNeighborComponent();

            // Find adjacent blocks
            neighbors.North = blockMap.GetValueOrDefault(gridPos + new Vector2Int(0, -1));
            neighbors.South = blockMap.GetValueOrDefault(gridPos + new Vector2Int(0, 1));
            neighbors.East = blockMap.GetValueOrDefault(gridPos + new Vector2Int(1, 0));
            neighbors.West = blockMap.GetValueOrDefault(gridPos + new Vector2Int(-1, 0));

            // Count valid neighbors
            neighbors.NeighborCount = 0;
            if (neighbors.North.Id != Guid.Empty) neighbors.NeighborCount++;
            if (neighbors.South.Id != Guid.Empty) neighbors.NeighborCount++;
            if (neighbors.East.Id != Guid.Empty) neighbors.NeighborCount++;
            if (neighbors.West.Id != Guid.Empty) neighbors.NeighborCount++;

            ntt.Set(ref neighbors);

            // Create physics for ALL blocks to ensure asteroid is solid
            // The optimization comes from other areas like structural integrity and neighbor tracking
            var worldPos = GridToWorld(gridPos, center);
            var bodyId = Box2DPhysicsWorld.CreateBoxBody(
                worldPos, 0f, true, 1.0f, 0.5f, 0.1f
            );

            var boxBody = new Box2DBodyComponent(
                ntt, bodyId, true,
                GetBlockColor(ntt.Get<AsteroidBlockComponent>()),
                ShapeType.Box, 1.0f
            );

            ntt.Set(ref boxBody);

            // Update block component
            ref var asteroidBlock = ref ntt.Get<AsteroidBlockComponent>();
            asteroidBlock.HasPhysics = true;
        }

        // Phase 3: Calculate initial structural integrity
        foreach (var ntt in entities)
        {
            if (!ntt.Has<AsteroidNeighborComponent>()) continue;

            var neighbors = ntt.Get<AsteroidNeighborComponent>();
            var distance = CalculateAnchorDistance(ntt, neighbors);

            var integrity = new StructuralIntegrityComponent
            {
                SupportDistance = distance,
                Integrity = 1f - (distance / 8f),
                NeedsRecalculation = false
            };

            ntt.Set(ref integrity);
        }

        var physicsCount = entities.Count(e => e.Has<Box2DBodyComponent>());
        Console.WriteLine($"Created asteroid {asteroidId} with {entities.Count} blocks (all with physics for solid structure)");
        return entities;
    }

    private static Vector2Int ToGridPos(Vector2 worldPos, Vector2 center)
    {
        return new Vector2Int((int)(worldPos.X - center.X), (int)(worldPos.Y - center.Y));
    }

    private static Vector2 GridToWorld(Vector2Int gridPos, Vector2 center)
    {
        return new Vector2(center.X + gridPos.X, center.Y + gridPos.Y);
    }

    private static bool IsNearCenter(Vector2 position, Vector2 center, float anchorRadius)
    {
        var distance = Vector2.Distance(position, center);
        return distance <= anchorRadius;
    }

    private static uint GetBlockColor(AsteroidBlockComponent blockData)
    {
        // Default asteroid block color
        return 0xFF808080; // Gray
    }


    private static int CalculateAnchorDistance(NTT start, AsteroidNeighborComponent neighbors)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<(NTT block, int distance)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (current.Id == Guid.Empty || visited.Contains(current.Id))
                continue;

            visited.Add(current.Id);

            // Check if this block is an anchor
            if (current.Has<AsteroidBlockComponent>())
            {
                var block = current.Get<AsteroidBlockComponent>();
                if (block.IsAnchor)
                    return distance;
            }

            // Check neighbors
            if (current.Has<AsteroidNeighborComponent>())
            {
                var currentNeighbors = current.Get<AsteroidNeighborComponent>();

                if (currentNeighbors.North.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.North, distance + 1));
                if (currentNeighbors.South.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.South, distance + 1));
                if (currentNeighbors.East.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.East, distance + 1));
                if (currentNeighbors.West.Id != Guid.Empty)
                    queue.Enqueue((currentNeighbors.West, distance + 1));
            }
        }

        return int.MaxValue; // No anchor found
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
        var spawnPoint = new Vector2(x, y);
        return spawnPoint;
    }
}