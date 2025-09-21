# Asteroid Feature - Technical Design Document

## Overview
Players spawn inside hollowed-out asteroids that they must mine through to escape into the larger game universe. This document describes the optimized implementation using a structural integrity system with edge-only physics optimization, plus support for rotatable block-based ship construction within asteroids.

## Problem Statement
- Need destructible asteroids with thousands of blocks
- Creating individual Box2D bodies for each block causes severe performance issues
- Must prevent floating island chunks when players cut through sections
- Should provide engaging gameplay with strategic destruction
- Need ship building system with directional components (engines, weapons, shields)
- Components must support rotation for optimal ship design

## Solution: Hybrid ECS with Structural Integrity + Rotatable Blocks

### Core Architecture

#### 1. Edge-Only Physics Optimization
- **Only edge blocks get Box2D bodies** (blocks exposed to space)
- Interior blocks exist as entities but have no physics bodies
- ~95% reduction in physics bodies (400 edge blocks vs 7000+ total for 60-radius asteroid)

#### 2. Structural Integrity System
- Blocks require support from "anchor" blocks or collapse
- Maximum support distance: 6-8 blocks from anchor
- Prevents floating islands - unsupported sections crumble
- Creates strategic gameplay - destroy support structures for chain reactions

#### 3. Rotatable Block System
- Blocks can be placed in 4 orientations: 0°, 90°, 180°, 270°
- Directional components (engines, weapons, shields) use block rotation
- Ship designs can optimize thrust vectors and defensive coverage
- Rotation affects visual representation and functionality, not adjacency

### Components

```csharp
[Component]
public struct AsteroidBlockComponent
{
    public int AsteroidId;
    public bool IsAnchor;      // Core blocks that provide support
    public bool HasPhysics;     // Currently has Box2D body
}

[Component]
public struct AsteroidNeighborComponent
{
    public NTT North, South, East, West;
    public int NeighborCount;  // Quick edge detection

    // Helper methods
    public bool IsEdge => NeighborCount < 4;

    public void ClearDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: North = default; break;
            case Direction.South: South = default; break;
            case Direction.East: East = default; break;
            case Direction.West: West = default; break;
        }
    }
}

[Component]
public struct StructuralIntegrityComponent
{
    public int SupportDistance;     // Distance to nearest anchor
    public float Integrity;         // 0-1, visual cracking
    public bool NeedsRecalculation;
}

// Tag component for collapse
[Component]
public struct StructuralCollapseComponent { }

[Component]
public struct GridPositionComponent
{
    public Vector2Int GridPos;
    public NTT Assembly;          // Ship this block belongs to
    public BlockRotation Rotation;  // 0, 90, 180, 270 degrees
}

public enum BlockRotation
{
    None = 0,      // Facing right (+X)
    Rotate90 = 1,  // Facing down (+Y in Box2D)
    Rotate180 = 2, // Facing left (-X)
    Rotate270 = 3  // Facing up (-Y in Box2D)
}

// Extension methods for rotation
public static class BlockRotationExtensions
{
    public static float ToRadians(this BlockRotation rotation)
        => rotation switch
        {
            BlockRotation.None => 0f,
            BlockRotation.Rotate90 => MathF.PI / 2f,
            BlockRotation.Rotate180 => MathF.PI,
            BlockRotation.Rotate270 => 3f * MathF.PI / 2f,
            _ => 0f
        };

    public static Vector2 GetDirection(this BlockRotation rotation)
        => rotation switch
        {
            BlockRotation.None => Vector2.UnitX,        // Right
            BlockRotation.Rotate90 => Vector2.UnitY,    // Down (Box2D coords)
            BlockRotation.Rotate180 => -Vector2.UnitX,  // Left
            BlockRotation.Rotate270 => -Vector2.UnitY,  // Up
            _ => Vector2.UnitX
        };

    public static BlockRotation RotateClockwise(this BlockRotation rotation)
        => (BlockRotation)(((int)rotation + 1) % 4);

    public static BlockRotation RotateCounterClockwise(this BlockRotation rotation)
        => (BlockRotation)(((int)rotation + 3) % 4);
}
```

### Directional Ship Components

```csharp
[Component]
public struct EngineComponent
{
    public float Thrust;
    public float PowerDraw;
    // Thrust direction determined by GridPositionComponent.Rotation
}

[Component]
public struct WeaponMountComponent
{
    public WeaponType Type;
    public float Damage;
    public float FireRate;
    public float Range;
    // Fire direction determined by GridPositionComponent.Rotation
}

[Component]
public struct DirectionalShieldComponent
{
    public float MaxShield;
    public float RegenRate;
    public float Arc;  // Coverage arc in radians (e.g., PI/2 for 90°)
    // Shield direction determined by GridPositionComponent.Rotation
}

[Component]
public struct HullComponent
{
    public float Mass;
    public float Armor;
}

[Component]
public struct AssemblyComponent
{
    public bool IsMobile;      // Can this ship move?
    public float TotalMass;    // Combined mass of all blocks
    public Vector2 CenterOfMass;
}
```

### Systems (Simplified Architecture)

#### 1. **AsteroidNeighborTrackingSystem**
Handles all neighbor updates when blocks are destroyed, directly creating physics for newly exposed edges.

```csharp
public class AsteroidNeighborTrackingSystem : NttSystem<DeathTagComponent, AsteroidNeighborComponent>
{
    public override void Update(in NTT ntt, ref DeathTagComponent death, ref AsteroidNeighborComponent neighbors)
    {
        // Update each neighbor when this block dies
        UpdateNeighborReference(neighbors.North, Direction.South);
        UpdateNeighborReference(neighbors.South, Direction.North);
        UpdateNeighborReference(neighbors.East, Direction.West);
        UpdateNeighborReference(neighbors.West, Direction.East);
    }

    private void UpdateNeighborReference(NTT neighbor, Direction fromDirection)
    {
        if (!neighbor.IsValid) return;

        ref var neighborRefs = ref neighbor.Get<AsteroidNeighborComponent>();

        // Clear the reference to the dead block
        neighborRefs.ClearDirection(fromDirection);
        neighborRefs.NeighborCount--;

        // If neighbor is now an edge and doesn't have physics, create it immediately
        if (neighborRefs.IsEdge && !neighbor.Has<Box2DBodyComponent>())
        {
            // Get position from transform or stored data
            var pos = neighbor.Get<TransformComponent>().Position;
            var blockData = neighbor.Get<AsteroidBlockComponent>();

            // Create static Box2D body for this newly exposed edge
            var bodyId = Box2DPhysicsWorld.CreateBoxBody(
                position: pos,
                rotation: 0f,
                isStatic: true,
                density: 1.0f,
                friction: 0.5f,
                restitution: 0.1f
            );

            var boxBody = new Box2DBodyComponent(
                neighbor,
                bodyId,
                isStatic: true,
                color: GetBlockColor(blockData),
                ShapeType.Box,
                density: 1.0f
            );

            neighbor.Set(ref boxBody);
            blockData.HasPhysics = true;
            neighbor.Set(ref blockData);
        }

        // Mark for structural integrity recalculation
        if (neighbor.Has<StructuralIntegrityComponent>())
        {
            ref var integrity = ref neighbor.Get<StructuralIntegrityComponent>();
            integrity.NeedsRecalculation = true;
        }
    }
}
```

#### 2. **AsteroidStructuralIntegritySystem**
Calculates support distances and marks unsupported blocks for collapse.

```csharp
public class AsteroidStructuralIntegritySystem : NttSystem<StructuralIntegrityComponent, AsteroidNeighborComponent>
{
    private const int MAX_SUPPORT_DISTANCE = 6;

    public override void Update(in NTT ntt, ref StructuralIntegrityComponent integrity, ref AsteroidNeighborComponent neighbors)
    {
        if (!integrity.NeedsRecalculation) return;

        // BFS to find nearest anchor
        var distance = CalculateDistanceToAnchor(ntt, neighbors);

        integrity.SupportDistance = distance;
        integrity.Integrity = 1f - (distance / (float)MAX_SUPPORT_DISTANCE);
        integrity.NeedsRecalculation = false;

        // Mark for collapse if too far from support
        if (distance > MAX_SUPPORT_DISTANCE)
        {
            ntt.Set(new StructuralCollapseComponent());

            // Visual feedback before collapse
            if (ntt.Has<Box2DBodyComponent>())
            {
                ref var body = ref ntt.Get<Box2DBodyComponent>();
                // Add red tint or crack texture
                body.Color = LerpColor(body.Color, 0xFF0000, 0.5f);
            }
        }
    }

    private int CalculateDistanceToAnchor(NTT start, AsteroidNeighborComponent neighbors)
    {
        var visited = new HashSet<uint>();
        var queue = new Queue<(NTT block, int distance)>();
        queue.Enqueue((start, 0));

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();

            if (!current.IsValid || visited.Contains(current.Id))
                continue;

            visited.Add(current.Id);

            // Check if this block is an anchor
            if (current.Has<AsteroidBlockComponent>())
            {
                var block = current.Get<AsteroidBlockComponent>();
                if (block.IsAnchor)
                    return distance; // Found nearest anchor!
            }

            // Check neighbors
            if (current.Has<AsteroidNeighborComponent>())
            {
                var currentNeighbors = current.Get<AsteroidNeighborComponent>();

                if (currentNeighbors.North.IsValid)
                    queue.Enqueue((currentNeighbors.North, distance + 1));
                if (currentNeighbors.South.IsValid)
                    queue.Enqueue((currentNeighbors.South, distance + 1));
                if (currentNeighbors.East.IsValid)
                    queue.Enqueue((currentNeighbors.East, distance + 1));
                if (currentNeighbors.West.IsValid)
                    queue.Enqueue((currentNeighbors.West, distance + 1));
            }
        }

        return int.MaxValue; // No anchor found - will collapse
    }
}
```

#### 3. **ShipPropulsionSystem**
Handles engine thrust based on block rotation.

```csharp
public class ShipPropulsionSystem : NttSystem<AssemblyComponent>
{
    public override void Update(in NTT ship, ref AssemblyComponent assembly)
    {
        if (!assembly.IsMobile) return;

        Vector2 totalThrust = Vector2.Zero;
        Vector2 totalTorque = Vector2.Zero;

        foreach (var block in GetBlocksOfAssembly(ship))
        {
            if (!block.Has<EngineComponent>()) continue;

            var engine = block.Get<EngineComponent>();
            var gridPos = block.Get<GridPositionComponent>();

            // Engine thrust direction based on block rotation
            var thrustDirection = gridPos.Rotation.GetDirection();
            var thrustForce = thrustDirection * engine.Thrust;

            totalThrust += thrustForce;

            // Calculate torque for off-center engines
            var blockWorldPos = GridToWorld(gridPos.GridPos, ship);
            var leverArm = blockWorldPos - ship.Position;
            var torque = Vector2.Cross(leverArm, thrustForce);
            totalTorque += torque;
        }

        // Apply combined forces to ship
        if (ship.Has<Box2DBodyComponent>())
        {
            var body = ship.Get<Box2DBodyComponent>();
            body.ApplyForce(totalThrust);
            body.ApplyTorque(totalTorque.Z);  // Rotational force
        }
    }
}
```

#### 4. **WeaponSystem with Rotation Support**
Handles weapon firing based on block rotation.

```csharp
public class WeaponSystem : NttSystem<WeaponMountComponent, GridPositionComponent>
{
    public override void Update(in NTT weaponBlock, ref WeaponMountComponent weapon, ref GridPositionComponent gridPos)
    {
        if (!weapon.CanFire()) return;

        // Get world position and rotation
        var assembly = gridPos.Assembly;
        var worldPos = GridToWorld(gridPos.GridPos, assembly);

        // Combine block rotation with ship rotation
        var shipRotation = assembly.Has<Box2DBodyComponent>()
            ? assembly.Get<Box2DBodyComponent>().Rotation
            : 0f;

        var totalRotation = shipRotation + gridPos.Rotation.ToRadians();
        var fireDirection = new Vector2(MathF.Cos(totalRotation), MathF.Sin(totalRotation));

        // Spawn projectile in the correct direction
        SpawnProjectile(worldPos, fireDirection, weapon.Damage, weapon.Range);
    }
}
```

#### 5. **DirectionalShieldSystem**
Handles directional shield protection based on block rotation.

```csharp
public class DirectionalShieldSystem : NttSystem<DirectionalShieldComponent, GridPositionComponent>
{
    public bool CheckShieldCoverage(NTT shieldBlock, Vector2 incomingDirection)
    {
        var shield = shieldBlock.Get<DirectionalShieldComponent>();
        var gridPos = shieldBlock.Get<GridPositionComponent>();

        // Get shield facing direction
        var shieldDirection = gridPos.Rotation.GetDirection();

        // Check if incoming damage is within shield arc
        var angle = Vector2.Angle(shieldDirection, -incomingDirection);

        return angle <= shield.Arc / 2f;  // Within coverage arc?
    }
}
```

### Ship Building System

```csharp
public static class ShipBuilder
{
    public static void PlaceBlock(NTT ship, Vector2Int gridPos, BlockRotation rotation, Action<NTT> configureBlock)
    {
        var block = NttWorld.CreateEntity();

        // Core components with rotation
        block.Set(new GridPositionComponent
        {
            GridPos = gridPos,
            Assembly = ship,
            Rotation = rotation  // Store rotation!
        });
        block.Set(new HealthComponent(block, 50, 50));
        block.Set(new HullComponent { Mass = 1f });
        block.Set(new NetSyncComponent(block, SyncThings.Position | SyncThings.Health));

        // Let caller add specific components
        configureBlock(block);

        // Set up neighbors (rotation doesn't affect adjacency)
        UpdateBlockNeighbors(block, gridPos, ship);

        // Update ship's compound physics
        RecreateShipCompoundBody(ship);
    }

    // Build mode UI would call this:
    public static void RotateBlockInPlace(NTT block)
    {
        if (!block.Has<GridPositionComponent>()) return;

        ref var gridPos = ref block.Get<GridPositionComponent>();
        gridPos.Rotation = gridPos.Rotation.RotateClockwise();

        // Update visual representation
        if (block.Has<Box2DBodyComponent>())
        {
            var newRotation = gridPos.Rotation.ToRadians();
            block.Get<Box2DBodyComponent>().SetRotation(newRotation);
        }

        // Recreate ship physics if needed
        RecreateShipCompoundBody(gridPos.Assembly);
    }
}
```

### Initial Generation Integration

```csharp
// In SpawnManager.cs
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
        var gridPos = ToGridPos(block.Position);
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
        if (neighbors.North.IsValid) neighbors.NeighborCount++;
        if (neighbors.South.IsValid) neighbors.NeighborCount++;
        if (neighbors.East.IsValid) neighbors.NeighborCount++;
        if (neighbors.West.IsValid) neighbors.NeighborCount++;

        ntt.Set(ref neighbors);

        // Create physics ONLY for edge blocks
        if (neighbors.IsEdge)
        {
            var worldPos = GridToWorld(gridPos);
            var bodyId = Box2DPhysicsWorld.CreateBoxBody(
                worldPos, 0f, true, 1.0f, 0.5f, 0.1f
            );

            var boxBody = new Box2DBodyComponent(
                ntt, bodyId, true,
                GetBlockColor(ntt.Get<AsteroidBlockComponent>()),
                ShapeType.Box, 1.0f
            );
            boxBody.SyncFromBox2D();

            ntt.Set(ref boxBody);

            // Update block component
            ref var asteroidBlock = ref ntt.Get<AsteroidBlockComponent>();
            asteroidBlock.HasPhysics = true;
        }
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

    Console.WriteLine($"Created asteroid {asteroidId} with {entities.Count} blocks ({blockMap.Count(kvp => kvp.Value.Has<Box2DBodyComponent>())} with physics)");
    return entities;
}
```

### Compound Body with Rotated Fixtures

```csharp
public static void CreateShipCompoundBody(NTT ship, List<NTT> blocks)
{
    var fixtures = new List<(B2Polygon shape, Vector2 offset, float rotation)>();

    foreach (var block in blocks)
    {
        var gridPos = block.Get<GridPositionComponent>();
        var localPos = GridToLocal(gridPos.GridPos);
        var rotation = gridPos.Rotation.ToRadians();

        // Create rotated box fixture
        var box = b2MakeBox(0.5f, 0.5f);
        fixtures.Add((box, localPos, rotation));
    }

    // Create single body with all rotated fixtures
    Box2DPhysicsWorld.CreateCompoundBodyWithRotations(ship, fixtures);
}
```

### Visual Representation with Rotation

```csharp
// For rendering blocks with rotation indicators
public class BlockRenderSystem : NttSystem<GridPositionComponent>
{
    public override void Update(in NTT block, ref GridPositionComponent gridPos)
    {
        var worldPos = GridToWorld(gridPos.GridPos, gridPos.Assembly);
        var rotation = gridPos.Rotation.ToRadians();

        // Draw block with rotation
        DrawSprite(block, worldPos, rotation);

        // Draw direction indicator for engines/weapons
        if (block.Has<EngineComponent>())
        {
            var exhaustDirection = gridPos.Rotation.GetDirection() * -1; // Opposite of thrust
            DrawThrustIndicator(worldPos, exhaustDirection);
        }

        if (block.Has<WeaponMountComponent>())
        {
            var fireDirection = gridPos.Rotation.GetDirection();
            DrawWeaponBarrel(worldPos, fireDirection);
        }
    }
}
```

### System Registration

```csharp
// In Game.cs static constructor
var systems = new List<NttSystem>
{
    // Existing systems...
    new SpawnSystem(),
    new ViewportSystem(),
    new InputSystem(),

    // Asteroid systems - ORDER MATTERS!
    new AsteroidNeighborTrackingSystem(),    // Must run before integrity
    new AsteroidStructuralIntegritySystem(), // Must run before collapse
    new AsteroidCollapseSystem(),           // Must run before death
    new AsteroidDebrisSystem(),             // Can run anytime

    // Ship systems
    new ShipPropulsionSystem(),             // Handle thrust from engines
    new WeaponSystem(),                     // Handle directional weapons
    new DirectionalShieldSystem(),          // Handle shield coverage
    new BlockRenderSystem(),                // Visual representation

    // Continue with existing systems...
    new DamageSystem(),
    new HealthSystem(),
    new DropSystem(),
    new DeathSystem(),    // Processes DeathTagComponent from collapses
    // ...
};
```

### Block Types & Resources

```csharp
public enum AsteroidBlockType
{
    Stone = 0,      // Common, 20 HP, drops 1 resource
    IronOre = 1,    // 15% chance, 30 HP, drops 2 resources
    CopperOre = 2,  // 10% chance, 25 HP, drops 2 resources
    RareOre = 3     // 5% chance, 50 HP, drops 5 resources
}
```

## Example Ship Design

Ship layout with rotations:
```
  H   W→  H      (W→ = weapon facing right)
  H   H   H
E↑ E↑ C  E↓ E↓   (E↑ = engine facing up, E↓ = engine facing down)
  H   H   H      (C = cockpit/core)
  H  S←  H       (S← = shield facing left)
```

This creates:
- Forward thrust from bottom engines (E↓)
- Reverse thrust from top engines (E↑)
- Weapons firing forward (W→)
- Rear shield protection (S←)

## Performance Metrics

### Before Optimization
- 7000+ Box2D bodies for 60-radius asteroid
- Severe frame drops
- O(n²) collision checks in worst case

### After Optimization
- ~400 Box2D bodies (edges only)
- 95% reduction in physics overhead
- O(1) neighbor lookups via component references
- Localized updates on block destruction

## Gameplay Flow

1. **Player Spawns**: Inside 10x10 hollow area in asteroid center
2. **Mining**: Players shoot blocks to destroy them
3. **Ship Building**: Players construct ships with rotatable components inside hollows
4. **Physics Activation**: Interior blocks become edges, get physics automatically
5. **Structural Collapse**: Unsupported sections crumble with delay
6. **Resource Collection**: Destroyed blocks drop resources (reduced for collapses)
7. **Escape**: Player breaks through to outer space with their constructed ship

## Configuration

```csharp
// Tunable parameters
const int MAX_SUPPORT_DISTANCE = 6;      // Blocks from anchor
const float COLLAPSE_DELAY = 0.1f;       // Between chain reactions
const float DEBRIS_LIFETIME = 3.0f;      // Seconds before cleanup
const int MIN_ASTEROID_SIZE = 10;        // Blocks before total collapse
const int ASTEROID_RADIUS = 60;          // Default size
const int HOLLOW_SIZE = 10;              // Spawn area size
```

## Implementation Status

### Completed
- ✅ AsteroidComponent for metadata
- ✅ AsteroidGenerator with FastNoiseLite
- ✅ Basic block creation with health/drops
- ✅ Integration with existing damage/death systems
- ✅ Documentation with full code examples
- ✅ Block rotation system design
- ✅ Directional components (engines, weapons, shields)

### Pending (Performance Optimization)
- ⏳ AsteroidNeighborTrackingSystem implementation
- ⏳ AsteroidStructuralIntegritySystem implementation
- ⏳ AsteroidCollapseSystem with chain reactions
- ⏳ AsteroidDebrisSystem with particle effects
- ⏳ Update SpawnManager.CreateAsteroid for edge-only physics
- ⏳ Ship building system with rotation support
- ⏳ Compound body creation with rotated fixtures
- ⏳ Test and tune support distance parameters

## Future Enhancements

1. **Multiple Asteroids**: Spawn several asteroids as starting bases
2. **Asteroid Fields**: Dense regions of smaller asteroids
3. **Gravity Wells**: Large asteroids affect nearby physics
4. **Ore Veins**: Connected deposits for bonus resources
5. **Hollow Caverns**: Natural caves inside larger asteroids
6. **Asteroid Bases**: Player-built structures anchored to asteroids
7. **Modular Ships**: Detachable sections for complex designs
8. **Resource Processing**: Refineries that process raw materials
9. **Ship Docking**: Magnetic docking for ship combination

## Technical Notes

### Why This Approach?
- **ECS-Native**: Uses components and systems, no external data structures
- **Event-Driven**: Updates only on block destruction, not every frame
- **Cache-Friendly**: Direct entity references in neighbor components
- **Scalable**: Can handle multiple asteroids simultaneously
- **Network-Friendly**: Components auto-sync via existing systems
- **Rotation-Aware**: Directional components work seamlessly with orientation

### Edge Cases Handled
- **Floating Islands**: Prevented via structural integrity - disconnected sections collapse
- **Chain Reactions**: Limited by collapse delay to prevent instant destruction
- **Minimum Size**: Prevents infinite fragmentation
- **Anchor Blocks**: Ensure some stability in asteroid core
- **Edge Detection**: Automatic physics creation for newly exposed surfaces
- **Rotation Conflicts**: Block rotation doesn't affect neighbor relationships
- **Compound Bodies**: Rotated fixtures in ship physics bodies

### Critical Integration Points

1. **DeathSystem Hook**: Must run after AsteroidNeighborTrackingSystem to process collapsed blocks
2. **Network Sync**: All asteroid components must include NetSyncComponent
3. **Collision Filtering**: Debris should not collide with other debris (performance)
4. **Entity Pooling**: Reuse debris entities to prevent allocation spam
5. **Spatial Partitioning**: Leverage existing Grid.cs for neighbor queries if needed
6. **Rotation Sync**: GridPositionComponent.Rotation must sync to clients for visual consistency

## Testing Checklist

- [ ] Verify edge detection works correctly
- [ ] Test chain collapse with strategic destruction
- [ ] Confirm physics bodies created/destroyed properly
- [ ] Check network sync for all block states
- [ ] Validate resource drops from different ore types
- [ ] Test performance with multiple asteroids
- [ ] Ensure debris cleanup after lifetime expires
- [ ] Verify structural integrity BFS algorithm
- [ ] Test anchor block placement and support distances
- [ ] Confirm no floating islands can exist
- [ ] Test block rotation in all 4 orientations
- [ ] Verify directional components work with rotation
- [ ] Test ship building system with rotated blocks
- [ ] Confirm compound body creation with rotated fixtures
- [ ] Validate engine thrust directions with rotation
- [ ] Test weapon firing directions with rotation
- [ ] Verify shield coverage calculations with rotation

## Code Location

- `/server/Simulation/Components/AsteroidComponent.cs` - Block component
- `/server/Helpers/AsteroidGenerator.cs` - Generation logic with FastNoiseLite
- `/server/Simulation/Managers/SpawnManager.cs` - CreateAsteroid method with neighbor setup
- `/server/Simulation/Systems/AsteroidNeighborTrackingSystem.cs` - Neighbor updates and physics creation
- `/server/Simulation/Systems/AsteroidStructuralIntegritySystem.cs` - Support calculation
- `/server/Simulation/Systems/AsteroidCollapseSystem.cs` - Collapse handling
- `/server/Simulation/Systems/AsteroidDebrisSystem.cs` - Debris management
- `/server/Simulation/Systems/ShipPropulsionSystem.cs` - Engine thrust handling
- `/server/Simulation/Systems/WeaponSystem.cs` - Directional weapon firing
- `/server/Simulation/Systems/DirectionalShieldSystem.cs` - Shield coverage
- `/server/Simulation/Systems/BlockRenderSystem.cs` - Visual representation