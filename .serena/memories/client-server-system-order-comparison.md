# Client vs Server System Execution Order Comparison

## Server System Order (server/Simulation/Game.cs)

The server runs **20 systems** in this exact order at 60 TPS:

1. **SpawnSystem** - Entity creation and spawner processing
2. **ViewportSystem** - Viewport culling for network optimization
3. **InputSystem** - Player input processing
4. **PositionSyncSystem** - Marks physics components as changed when position/rotation changes significantly
5. **ShipPhysicsRebuildSystem** - Rebuild Box2D bodies when ship parts change
6. **GravitySystem** - Apply gravity forces from gravity sources
7. **Box2DEngineSystem** - Process engine thrust and RCS using Box2D
8. **EnergySystem** - Energy generation, consumption, and battery management
9. **ShieldSystem** - Shield charge/recharge and power consumption
10. **WeaponSystem** - Weapon firing and projectile spawning
11. **PickupCollisionResolver** - Handle pickup collection
12. **ProjectileCollisionSystem** - Handle projectile collisions
13. **DamageSystem** - Apply damage to entities
14. **HealthSystem** - Process health regeneration and death
15. **DropSystem** - Handle entity drops on death
16. **LifetimeSystem** - Remove entities with expired lifetime
17. **LevelExpSystem** - Experience and leveling
18. **RespawnSystem** - Player respawn logic
19. **ComponentSyncSystem** - Generic component sync to clients
20. **DeathSystem** - Final cleanup for dead entities

## Client System Order (pixiejsClient/src/app/screens/game/GameScreen.ts)

The client runs **5 systems** in this order at variable FPS:

```typescript
World.setSystems(
    this.inputSystem,      // 1
    this.networkSystem,    // 2
    this.deathSystem,      // 3
    this.lifetimeSystem,   // 4
    this.particleSystem    // 5
);
```

**Note**: RenderSystem runs separately in `variableUpdate()` at display refresh rate (60+ FPS)

### Client System Details

1. **InputSystem** - Capture local player input, send to server
2. **NetworkSystem** - Apply server state updates (position, velocity, rotation)
3. **DeathSystem** - Handle entity death (destroy entities, dispatch events)
4. **LifetimeSystem** - Remove entities with expired lifetime
5. **ParticleSystem** - Update particle effects
6. **RenderSystem** - Render all entities (runs at display FPS, not game tick rate)

## Key Differences

### Missing from Client (Server-Only)

These systems run **only on the server** because the server is authoritative:

- **SpawnSystem** - Server creates entities
- **ViewportSystem** - Server-side viewport culling
- **PositionSyncSystem** - Server tracks position changes for network sync
- **ShipPhysicsRebuildSystem** - Server manages Box2D physics
- **GravitySystem** - Server simulates gravity
- **Box2DEngineSystem** - Server simulates engine physics
- **EnergySystem** - Server manages energy/power
- **ShieldSystem** - Server manages shields
- **WeaponSystem** - Server handles weapon firing
- **PickupCollisionResolver** - Server detects pickups
- **ProjectileCollisionSystem** - Server detects projectile hits
- **DamageSystem** - Server applies damage
- **HealthSystem** - Server manages health
- **DropSystem** - Server spawns drops
- **LevelExpSystem** - Server manages progression
- **RespawnSystem** - Server handles respawning
- **ComponentSyncSystem** - Server syncs components to clients

### Shared Systems (Both Client and Server)

1. **InputSystem**
   - **Server**: Processes received input packets, applies to entities
   - **Client**: Captures input, sends to server

2. **LifetimeSystem**
   - **Server**: Destroys entities when lifetime expires
   - **Client**: Destroys local entity copies when lifetime expires

3. **DeathSystem**
   - **Server**: Final cleanup, remove from physics world
   - **Client**: Destroy local entity, dispatch UI events

4. **NetworkSystem**
   - **Server**: N/A (server doesn't have this)
   - **Client**: Receives server updates, applies to local entities

### Client-Only Systems

- **NetworkSystem** - Receives and applies server state
- **ParticleSystem** - Visual effects only
- **RenderSystem** - Rendering (not in World.update loop)

## Execution Flow Comparison

### Server Flow (60 TPS)
```
NttWorld.UpdateSystems():
    OnBeginTick()
    for each of 20 systems:
        UpdateNTTs()  // Process entity changes
        system.BeginUpdate(deltaTime)
    UpdateNTTs()      // Final pass
    OnEndTick()
    Tick++
```

### Client Flow (Variable FPS)
```
World.update(deltaTime):  // Called from game loop
    OnBeginTick()
    for each of 5 systems:
        updateEntities()  // Process entity changes
        system.beginUpdate(deltaTime)
    updateEntities()      // Final pass
    onEndTick()
    currentTick++

RenderSystem.variableUpdate():  // Called separately at display FPS
    Render all entities
```

## System Order Philosophy

### Server Order Logic
Systems ordered by **dependency and causality**:
1. **Input → Physics → Simulation → Damage → Health → Cleanup**
2. Physics systems run before gameplay systems
3. Damage/Health systems run after collision detection
4. Death system runs last (after all gameplay)
5. Network sync happens just before death (ComponentSyncSystem)

### Client Order Logic
Systems ordered by **state update priority**:
1. **Input** - Send player actions first
2. **Network** - Receive server state updates
3. **Death** - Process deaths before cleanup
4. **Lifetime** - Clean up expired entities
5. **Particles** - Update visual effects

## Critical Ordering Insights

### Why DeathSystem is Last on Server
- Runs **after** ComponentSyncSystem (line 53 vs 54)
- Ensures clients receive death notification before entity cleanup
- Prevents race conditions where entity is destroyed before sync

### Why NetworkSystem is Second on Client
- Runs **after InputSystem** (send input first)
- Runs **before DeathSystem** (receive death notifications)
- Ensures server state applied before local processing

### Why LifetimeSystem Order Differs

**Server**: Line 50 - Between collision/health and experience systems
- Entities expire after taking damage but before respawn

**Client**: Line 4 - After death processing
- Local copies cleaned up after death events dispatched

## Performance Considerations

### Server
- **20 systems** × **1000+ entities** at **60 TPS** = High CPU load
- Uses multi-threading (ThreadedWorker) for parallel system execution
- Systems can specify thread count for work distribution

### Client  
- **5 systems** × **~100-500 entities** at **60 FPS** = Lower load
- Single-threaded (JavaScript constraint)
- RenderSystem runs at display FPS (potentially 120+ FPS)
- Network updates throttled by server tick rate (60 TPS)

## Recommendations for Alignment

### Current Status
✅ System architecture aligned (SystemN base classes)
✅ Update loop aligned (updateEntities between systems)
✅ Entity change tracking aligned (informChangesFor)

### Potential Improvements

1. **Add BuildModeSystem to World.update loop?**
   - Currently BuildModeSystem not in setSystems() call
   - Should it run in the main loop or stay separate?

2. **System Order Documentation**
   - Add comments explaining order rationale
   - Document which systems are client/server/shared

3. **Performance Monitoring**
   - Track system execution times
   - Identify bottlenecks in system order

4. **System Dependencies**
   - Document which systems depend on others
   - Validate order matches dependencies

## Conclusion

The client and server now share the **same ECS architecture** with aligned update loops, but have **different system sets** due to their roles:

- **Server**: Full simulation authority (20 systems)
- **Client**: State display and input capture (5 systems)

Both follow the same pattern:
```
UpdateLoop → For Each System → UpdateEntities → BeginUpdate
```

This alignment provides:
- Consistent development experience
- Easy system porting between client/server
- Shared understanding of execution order
- Type-safe component access
- Automatic entity filtering
