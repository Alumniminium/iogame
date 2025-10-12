# ECS Architecture Details

## Core ECS Framework (NttECS/)

### NTT (Entity)
- Lightweight entity struct with parent-child relationships
- Stores components in packed component storage
- Entity ID management and component access

### NttWorld
- Central ECS coordinator
- Manages entities, systems, and tick-based simulation
- Runs at 60 TPS (ticks per second)
- Global tick counter for change tracking

### NttSystem<T1, T2, ...>
- Base class for all systems
- Generic type parameters specify required components
- Automatic entity filtering based on component requirements
- Multi-threading support via `threads` parameter
- Override `Update(in NTT ntt, ref T1 c1, ref T2 c2, ...)` for logic

### PackedComponentStorage
- High-performance struct-of-arrays layout
- SIMD-friendly memory layout for cache efficiency
- Zero-allocation component access

## Component Lifecycle
1. Component created with initial ChangedTick = NttWorld.Tick
2. System modifies component data
3. If changed, system updates ChangedTick = NttWorld.Tick
4. ComponentSyncSystem detects changes and queues for network sync
5. Only changed components sent to clients

## System Execution Order (Game.cs)
Systems execute in strict order each tick:
1. SpawnSystem - Entity creation
2. ViewportSystem - Viewport culling
3. InputSystem - Player input
4. PositionSyncSystem - Position change detection
5. ShipPhysicsRebuildSystem - Physics body rebuilding
6. GravitySystem - Gravity forces
7. EngineSystem - Engine thrust/RCS via PhysicsWorld
8. EnergySystem - Energy management
9. ShieldSystem - Shield logic
10. WeaponSystem - Weapon firing
11. PickupCollisionResolver - Pickup collection
12. ProjectileCollisionSystem - Projectile collisions
13. DamageSystem - Damage application
14. HealthSystem - Health regen
15. DropSystem - Entity drops
16. LifetimeSystem - Lifetime expiry
17. LevelExpSystem - Experience/leveling
18. RespawnSystem - Player respawn
19. ComponentSyncSystem - Network sync
20. DeathSystem - Entity cleanup

**CRITICAL**: System order matters! Adding systems requires careful placement.

## Key Components

### PhysicsComponent
- Stores Box2D body ID and physics state
- Contains position, rotation, velocity
- Size, shape, color for rendering
- LastPosition/LastRotation for interpolation
- Synced to clients every tick when changed

### Other Core Components
- HealthComponent, EnergyComponent, ShieldComponent
- WeaponComponent, EngineComponent
- ViewportComponent, NetworkComponent
- ParentChildComponent for multi-part entities

## Performance Patterns

### Lock-Free Pooling (Pool<T>)
- Zero-allocation object pooling using Interlocked operations
- All storage pre-allocated at startup
- Thread-safe without locks

### SIMD Optimization (SwapList<T>)
- Vectorized Contains() and IndexOf() operations
- 8x-16x faster on vectorizable types
- Struct-of-arrays layout for SIMD

### Multi-Threading
- Systems with `threads > 1` automatically partition entities
- MultiThreadWorkQueue distributes work across threads
- Lock-free where possible