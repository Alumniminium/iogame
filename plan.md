# Particle System Implementation Plan

## Server Analysis Summary

After analyzing the server codebase, particles (EntityType.Pickable) exhibit the following behavior:

### Server Particle Characteristics
1. **Created by DropSystem**: When entities die, particles spawn with random colors, sizes, and velocities
2. **Physics Participation**: Particles DO participate in PhysicsSystem (unlike Static entities)
3. **Lifetime Management**: 5-10 second random lifetime via LifeTimeComponent → LifetimeSystem → DeathTagComponent
4. **Selective Collision**:
   - ✅ **Map boundary collision** (walls/floors/ceiling) - they bounce with elasticity
   - ✅ **Floor destruction** - destroyed when hitting Y boundaries (floor/ceiling)
   - ❌ **Entity-entity collision** - excluded from NarrowPhaseSystem
   - ❌ **Projectile collision** - projectiles pass through them
   - ✅ **Player pickup** - players can collect them via PickupCollisionResolver

### Key Server Components
- **EntityType.Pickable** - particles use this type
- **LifeTimeComponent** - stores `float LifeTimeSeconds`
- **LifetimeSystem** - decrements lifetime, adds DeathTagComponent when expired
- **PhysicsSystem** - handles movement, gravity, boundary bouncing, floor death
- **DropSystem** - spawns particles when entities die
- **SpawnManager.SpawnDrop** - creates particle entities

## Client Implementation Plan

### 1. EntityType Synchronization
**Problem**: Server and client have different EntityType enum values
- Server: `Pickable = 4`
- Client: `Pickable = 2`

**Action**: Verify network packet mapping handles this correctly, or fix enum alignment

### 2. Lifetime System Implementation

#### 2.1 Create LifetimeComponent
```typescript
// src/app/ecs/components/LifetimeComponent.ts
export class LifetimeComponent extends Component {
  public lifetimeSeconds: number;
  public originalLifetime: number;

  constructor(lifetimeSeconds: number) {
    super();
    this.lifetimeSeconds = lifetimeSeconds;
    this.originalLifetime = lifetimeSeconds;
  }
}
```

#### 2.2 Create LifetimeSystem
```typescript
// src/app/ecs/systems/LifetimeSystem.ts
export class LifetimeSystem extends System {
  public update(deltaTime: number): void {
    // Process entities with LifetimeComponent
    // Decrement lifetimeSeconds by deltaTime
    // Mark for destruction when <= 0 (add DeathTagComponent)
    // Handle fade-out transparency effect based on remaining lifetime
  }
}
```

### 3. Transparency Implementation

#### 3.1 Progressive Transparency
Implement fade-out effect as particles approach death:
- Calculate transparency based on `lifetimeSeconds / originalLifetime`
- Apply alpha to particle graphics: `alpha = Math.max(0.3, lifetimeRatio)`
- Particles start opaque and fade to 30% transparency before destruction

#### 3.2 Visual Distinctions
- Particles should be visually distinct from regular entities
- Consider particle-specific rendering effects (glow, sparkle, etc.)

### 4. Selective Collision Implementation

#### 4.1 Update PhysicsSystem
Ensure particles participate in physics movement:
- ✅ Apply gravity when near bottom of map
- ✅ Apply velocity integration and drag
- ✅ Handle map boundary collisions with bouncing
- ✅ Destroy particles when hitting floor/ceiling boundaries

#### 4.2 Update NarrowPhaseSystem
Particles must be excluded from entity-entity collision:
```typescript
// In NarrowPhaseSystem.update()
if (entity.type === EntityType.Static || entity.type === EntityType.Pickable) {
  return; // Skip collision processing
}
```

#### 4.3 Update ProjectileCollisionSystem (if exists)
Ensure projectiles pass through particles without destruction

### 5. Particle Creation Integration

#### 5.1 Network Packet Handling
Particles are created server-side and sent via network packets:
- Ensure NetworkSystem properly creates LifetimeComponent for Pickable entities
- Parse server lifetime data from network packets

#### 5.2 Local Particle Spawning (optional)
For immediate visual feedback, consider client-side particle prediction:
- When entities take damage/die, spawn temporary particles locally
- Replace with authoritative server particles when received

### 6. Pickup System Integration

#### 6.1 Player-Particle Collision
Maintain existing player pickup functionality:
- Players should still be able to collect particles
- Particles disappear when picked up (handled by server authority)

### 7. Rendering Optimizations

#### 7.1 Particle Batching
Particles are likely numerous and short-lived:
- Consider batch rendering for performance
- Use instancing for similar particle shapes
- Implement object pooling for particle graphics

#### 7.2 Culling
Particles excluded from ViewportSystem on server:
- Implement client-side culling for off-screen particles
- Only render particles within viewport bounds + margin

### 8. System Integration Order

Ensure proper system execution order:
1. **NetworkSystem** - receive particle updates from server
2. **PhysicsSystem** - move particles, handle boundary collisions
3. **LifetimeSystem** - decrement lifetime, apply transparency, mark for death
4. **CollisionSystems** - handle pickup interactions
5. **RenderSystem** - render particles with transparency

### 9. Implementation Checklist

- [ ] Fix EntityType enum alignment or verify packet mapping
- [ ] Create LifetimeComponent with lifetime tracking
- [ ] Implement LifetimeSystem with transparency fade-out
- [ ] Update PhysicsSystem to handle particle boundary collisions
- [ ] Update NarrowPhaseSystem to exclude particles from entity collisions
- [ ] Verify ProjectileCollisionSystem allows projectiles to pass through particles
- [ ] Test particle creation from network packets
- [ ] Implement progressive transparency rendering
- [ ] Verify player pickup functionality remains intact
- [ ] Add particle-specific visual effects (optional)
- [ ] Implement performance optimizations (batching, culling)


## Expected Outcome

Identical particle behavior to server:
- Particles spawn when entities die
- They bounce around the map with physics
- They become increasingly transparent over their 5-10 second lifetime
- They only collide with map boundaries (floors/walls/ceiling), not other entities
- Projectiles pass through them
- Players can pick them up
- They're destroyed when hitting floor/ceiling or when lifetime expires