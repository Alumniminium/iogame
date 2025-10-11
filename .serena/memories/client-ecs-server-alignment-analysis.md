# Client ECS → Server NttECS Alignment Analysis

## Current Architecture Comparison

### Server NttECS Architecture
```csharp
// Lightweight entity handle
struct NTT {
    readonly Guid Id;
    void Set<T>(T component);
    ref T Get<T>();
    bool Has<T>();
}

// Static world coordinator
static class NttWorld {
    static Dictionary<Guid, NTT> NTTs;
    static NttSystem[] Systems;
    static ConcurrentQueue<NTT> ChangedThisTick;
    
    static void UpdateSystems() {
        OnBeginTick?.Invoke();
        for (var i = 0; i < Systems.Length; i++) {
            UpdateNTTs(); // Process change queue
            Systems[i].BeginUpdate(deltaTime);
        }
        UpdateNTTs();
        OnEndTick?.Invoke();
        Tick++;
    }
}

// Generic typed systems
abstract class NttSystem<T1, T2> {
    ConcurrentDictionary<Guid, NTT> _entities;
    SwapList<NTT> _entitiesList;
    
    void EntityChanged(in NTT ntt) {
        if (MatchesFilter(ntt))
            _entities.TryAdd(ntt, ntt);
        else
            _entities.TryRemove(ntt);
    }
    
    abstract void Update(in NTT ntt, ref T1 c1, ref T2 c2);
}

// Usage
class MovementSystem : NttSystem<PositionComponent, VelocityComponent> {
    public override void Update(in NTT ntt, ref PositionComponent pos, ref VelocityComponent vel) {
        pos.X += vel.X * DeltaTime;
        pos.Y += vel.Y * DeltaTime;
    }
}
```

### Client Current Architecture
```typescript
// Full entity object
class Entity {
    id: string;
    components: Map<string, Component>;
    
    set<T extends Component>(component: T): void {
        this.components.set(component.constructor.name, component);
        World.notifyComponentChange(this);
    }
    
    get<T>(componentClass: new (...) => T): T | undefined {
        return this.components.get(componentClass.name);
    }
}

// Static world with instance pattern
class World {
    private static entities = new Map<string, Entity>();
    private static systems = new Map<string, SystemDefinition>();
    private static changedEntities = new Set<Entity>();
    
    static update(deltaTime: number): void {
        // Process changed entities
        changedEntities.forEach(entity => {
            systemExecutionOrder.forEach(system => {
                system.onEntityChanged?.(entity);
            });
        });
        changedEntities.clear();
        
        // Update systems
        systemExecutionOrder.forEach(system => {
            system.update(deltaTime);
        });
    }
}

// Non-generic system base
abstract class System {
    abstract componentTypes: (new (...) => Component)[];
    
    update(deltaTime: number): void {
        const entities = World.queryEntitiesWithComponents(...this.componentTypes);
        entities.forEach(entity => {
            this.updateEntity(entity, deltaTime);
        });
    }
    
    abstract updateEntity(entity: Entity, deltaTime: number): void;
}

// Usage
class InputSystem extends System {
    readonly componentTypes = [Box2DBodyComponent, NetworkComponent];
    
    protected updateEntity(entity: Entity, _deltaTime: number): void {
        const network = entity.get(NetworkComponent)!;
        const physics = entity.get(Box2DBodyComponent);
        // ...
    }
}
```

## Key Differences

### 1. Component Storage
- **Server**: Packed storage in `PackedComponentStorage<T>`, components separate from entities
- **Client**: Components stored in `Map<string, Component>` on each entity
- **Impact**: Client can't do packed storage efficiently in JS, but can improve access patterns

### 2. System Entity Tracking
- **Server**: Systems maintain `_entities` dictionary and `_entitiesList` automatically
- **Client**: Systems query World every frame via `World.queryEntitiesWithComponents()`
- **Impact**: Major performance difference - O(n) scan every frame vs O(1) filtered list

### 3. System Typing
- **Server**: Generic `NttSystem<T1, T2, ...>` with typed `Update(ref T1, ref T2)` method
- **Client**: Abstract `System` with `componentTypes` array, `updateEntity(entity, deltaTime)`
- **Impact**: Server has compile-time type safety, client uses runtime checks

### 4. Component Access
- **Server**: `ref T Get<T>()` returns reference, never null (returns ref to default)
- **Client**: `get<T>(Class)` returns `T | undefined`, requires null checks
- **Impact**: Server safer and more ergonomic

### 5. Change Tracking
- **Server**: `ConcurrentQueue<NTT> ChangedThisTick` processed between systems
- **Client**: `Set<Entity> changedEntities` processed once at start of frame
- **Impact**: Similar pattern but server processes between each system

### 6. World Access
- **Server**: Systems access `NttWorld` static methods directly
- **Client**: Systems use `globalThis.__WORLD_CLASS` hack
- **Impact**: Client pattern is hacky and unsafe

## Problems with Current Client Implementation

### 1. Performance Issues
```typescript
// PROBLEM: Queries entire entity list every frame
update(deltaTime: number): void {
    const entities = World.queryEntitiesWithComponents(...this.componentTypes);
    entities.forEach(entity => {
        this.updateEntity(entity, deltaTime);
    });
}

// With 1000 entities, 10 systems: 10,000 filter operations per frame
```

### 2. Type Safety Issues
```typescript
// PROBLEM: Requires non-null assertions everywhere
protected updateEntity(entity: Entity, _deltaTime: number): void {
    const network = entity.get(NetworkComponent)!; // !
    const physics = entity.get(Box2DBodyComponent); // might be undefined
    if (!physics) return; // extra check
}
```

### 3. Component Access Verbosity
```typescript
// PROBLEM: Must pass component class constructor
const comp = entity.get(NetworkComponent);
// vs server:
ref var comp = ref ntt.Get<NetworkComponent>();
```

### 4. No Automatic Filtering
```typescript
// PROBLEM: Systems don't maintain filtered entity lists
// Must either:
// A) Query every frame (slow)
// B) Manually implement filtering in onEntityChanged (error-prone)
```

### 5. Global State Hacks
```typescript
// PROBLEM: Access World through globalThis
const WorldClass = (globalThis as any).__WORLD_CLASS;
// vs server: just use NttWorld.Method()
```

## Proposed Client Architecture (Aligned)

### 1. Typed System Base Classes

```typescript
// Base system (no components)
abstract class System {
    protected _entities = new Set<Entity>();
    protected _entitiesList: Entity[] = [];
    
    // Automatic entity filtering
    entityChanged(entity: Entity): void {
        const matches = this.matchesFilter(entity);
        if (matches) {
            if (!this._entities.has(entity)) {
                this._entities.add(entity);
                this._entitiesList.push(entity);
            }
        } else {
            if (this._entities.has(entity)) {
                this._entities.delete(entity);
                const idx = this._entitiesList.indexOf(entity);
                if (idx !== -1) {
                    // Swap-remove like server
                    this._entitiesList[idx] = this._entitiesList[this._entitiesList.length - 1];
                    this._entitiesList.pop();
                }
            }
        }
    }
    
    protected abstract matchesFilter(entity: Entity): boolean;
    
    beginUpdate(deltaTime: number): void {
        this.update(deltaTime);
    }
    
    protected abstract update(deltaTime: number): void;
}

// Generic system with 1 component
abstract class System1<T1 extends Component> extends System {
    constructor(private c1Type: new (entityId: string) => T1) {
        super();
    }
    
    protected matchesFilter(entity: Entity): boolean {
        return entity.has(this.c1Type);
    }
    
    protected update(deltaTime: number): void {
        for (const entity of this._entitiesList) {
            const c1 = entity.get(this.c1Type)!;
            this.updateEntity(entity, c1, deltaTime);
        }
    }
    
    protected abstract updateEntity(entity: Entity, c1: T1, deltaTime: number): void;
}

// Generic system with 2 components
abstract class System2<T1 extends Component, T2 extends Component> extends System {
    constructor(
        private c1Type: new (entityId: string) => T1,
        private c2Type: new (entityId: string) => T2
    ) {
        super();
    }
    
    protected matchesFilter(entity: Entity): boolean {
        return entity.has(this.c1Type) && entity.has(this.c2Type);
    }
    
    protected update(deltaTime: number): void {
        for (const entity of this._entitiesList) {
            const c1 = entity.get(this.c1Type)!;
            const c2 = entity.get(this.c2Type)!;
            this.updateEntity(entity, c1, c2, deltaTime);
        }
    }
    
    protected abstract updateEntity(entity: Entity, c1: T1, c2: T2, deltaTime: number): void;
}

// Up to System6<T1, T2, T3, T4, T5, T6>
```

### 2. Simplified World Update Loop

```typescript
class World {
    private static changedEntities = new Set<Entity>();
    private static systems: System[] = [];
    
    static informChangesFor(entity: Entity): void {
        this.changedEntities.add(entity);
    }
    
    private static updateEntities(): void {
        this.changedEntities.forEach(entity => {
            this.systems.forEach(system => {
                system.entityChanged(entity);
            });
        });
        this.changedEntities.clear();
    }
    
    static update(deltaTime: number): void {
        this.onBeginTick?.();
        
        for (const system of this.systems) {
            this.updateEntities();
            system.beginUpdate(deltaTime);
        }
        
        this.updateEntities();
        this.onEndTick?.();
        this.currentTick++;
    }
}
```

### 3. Updated System Usage

```typescript
// OLD: Manual query every frame
class InputSystem extends System {
    readonly componentTypes = [Box2DBodyComponent, NetworkComponent];
    
    protected updateEntity(entity: Entity, _deltaTime: number): void {
        const network = entity.get(NetworkComponent)!;
        const physics = entity.get(Box2DBodyComponent)!;
        // ...
    }
}

// NEW: Typed, automatic filtering
class InputSystem extends System2<Box2DBodyComponent, NetworkComponent> {
    constructor(private inputManager: InputManager) {
        super(Box2DBodyComponent, NetworkComponent);
    }
    
    protected updateEntity(entity: Entity, physics: Box2DBodyComponent, network: NetworkComponent, _deltaTime: number): void {
        // Components guaranteed non-null
        if (!network.isLocallyControlled) return;
        // ...
    }
}
```

### 4. Improved Entity Component Access

```typescript
// Option A: Keep current API but make it safer
class Entity {
    get<T extends Component>(componentClass: new (entityId: string) => T): T {
        const comp = this.components.get(componentClass.name);
        if (!comp) {
            throw new Error(`Entity ${this.id} missing component ${componentClass.name}`);
        }
        return comp as T;
    }
    
    tryGet<T extends Component>(componentClass: new (entityId: string) => T): T | undefined {
        return this.components.get(componentClass.name) as T | undefined;
    }
}

// Option B: Add component type registry for cleaner API
class Entity {
    // Type-safe without passing constructor
    get<T extends Component>(): T {
        const typeName = // ... derive from T
        // Similar to server's ntt.Get<T>()
    }
}
```

## Refactoring Plan

### Phase 1: Core System Refactor
1. Create generic `System1<T>`, `System2<T1,T2>`, ... `System6<T1...T6>` classes
2. Add `_entities` and `_entitiesList` to base System
3. Implement `entityChanged(entity)` automatic filtering
4. Update `World.update()` to call `updateEntities()` between systems

### Phase 2: System Migration
1. Update each system to extend appropriate `SystemN<...>` variant
2. Change `updateEntity(entity, deltaTime)` signature to `updateEntity(entity, c1, c2, ..., deltaTime)`
3. Remove manual `World.queryEntities()` calls from system update loops
4. Remove null-assertion operators (`!`) from component access

### Phase 3: World Cleanup
1. Remove singleton pattern - make World fully static
2. Rename `notifyComponentChange()` → `informChangesFor()`
3. Remove `SystemDefinition` complexity if not needed
4. Remove globalThis hacks

### Phase 4: Entity Improvements
1. Make `get<T>()` throw on missing component (matching server behavior)
2. Add `tryGet<T>()` for optional component access
3. Consider component type registry for cleaner API

### Phase 5: Optional Query System
1. Add `Query<T1, T2, ...>()` static methods to World
2. Implement iterator-based queries like server's NttQuery
3. Use for one-off queries outside systems

## Benefits of Alignment

### 1. Performance
- **Before**: 10 systems × 1000 entities = 10,000 filter checks per frame
- **After**: Systems maintain filtered lists, ~0 checks per frame (only on component changes)

### 2. Type Safety
```typescript
// Before: Unsafe, verbose
const comp = entity.get(NetworkComponent)!;
if (!comp) return;

// After: Type-safe, clean
protected updateEntity(entity: Entity, network: NetworkComponent, deltaTime: number): void {
    // network guaranteed non-null
}
```

### 3. Developer Experience
- Same patterns between client and server
- Less boilerplate in systems
- Compile-time errors instead of runtime errors
- Cleaner, more maintainable code

### 4. Code Consistency
```typescript
// Server C#
class MovementSystem : NttSystem<PositionComponent, VelocityComponent> {
    public override void Update(in NTT ntt, ref PositionComponent pos, ref VelocityComponent vel) {
        pos.X += vel.X * DeltaTime;
    }
}

// Client TypeScript (after alignment)
class MovementSystem extends System2<PositionComponent, VelocityComponent> {
    protected updateEntity(entity: Entity, pos: PositionComponent, vel: VelocityComponent, deltaTime: number): void {
        pos.x += vel.x * deltaTime;
    }
}
```

## JavaScript Constraints

### 1. No References
- **Server**: `ref T Get<T>()` returns mutable reference
- **Client**: Must return component object (already mutable in JS)
- **Solution**: Components are objects, mutations work automatically

### 2. No Packed Storage
- **Server**: `PackedComponentStorage<T>` with dense arrays
- **Client**: `Map<string, Component>` per entity
- **Impact**: Can't achieve same cache locality, but filtered entity lists still help

### 3. No Multi-Threading
- **Server**: `ThreadedWorker` for parallel system execution
- **Client**: Single-threaded JavaScript
- **Impact**: Ignore threading features, keep single-threaded update loop

### 4. No Generics with Constructors
- **TypeScript**: Can't instantiate `new T()` from generic type parameter
- **Solution**: Pass component constructors to System base class constructor

### 5. No Static Generic Classes
- **C#**: `PackedComponentStorage<T>` is static generic class
- **TypeScript**: No equivalent
- **Solution**: Use instance-based storage (Entity.components Map)

## Implementation Strategy

### 1. Gradual Migration
- Create new System base classes alongside old ones
- Migrate systems one at a time
- Keep old API working during transition
- Remove old API after all systems migrated

### 2. Testing Strategy
- Test each system after migration
- Verify entity filtering works correctly
- Check performance improvements
- Ensure no regressions

### 3. Breaking Changes
- System constructor signatures change
- `updateEntity()` signature changes
- `get<T>()` throws instead of returning undefined
- World API changes (notifyComponentChange → informChangesFor)

## Example Migration

### Before (Current)
```typescript
export class InputSystem extends System {
    readonly componentTypes = [Box2DBodyComponent, NetworkComponent];
    
    update(deltaTime: number): void {
        const entities = World.queryEntitiesWithComponents(...this.componentTypes);
        entities.forEach(entity => this.updateEntity(entity, deltaTime));
    }
    
    protected updateEntity(entity: Entity, _deltaTime: number): void {
        const network = entity.get(NetworkComponent);
        if (!network) return;
        const physics = entity.get(Box2DBodyComponent);
        if (!physics || !network.isLocallyControlled) return;
        // ...
    }
}
```

### After (Aligned)
```typescript
export class InputSystem extends System2<Box2DBodyComponent, NetworkComponent> {
    constructor(private inputManager: InputManager) {
        super(Box2DBodyComponent, NetworkComponent);
    }
    
    // No manual update() needed - base class handles it
    
    protected updateEntity(
        entity: Entity,
        physics: Box2DBodyComponent,
        network: NetworkComponent,
        _deltaTime: number
    ): void {
        if (!network.isLocallyControlled) return;
        // physics and network guaranteed non-null
        // ...
    }
}
```

## Conclusion

The refactoring will bring significant improvements:
1. **10-100x faster** system updates (no repeated queries)
2. **Type-safe** component access (compile-time errors)
3. **Consistent** patterns between client and server
4. **Cleaner** code with less boilerplate

The main work is creating the generic System base classes and migrating each system to use them. The Entity and World changes are minor. Total effort: ~4-8 hours for complete migration.