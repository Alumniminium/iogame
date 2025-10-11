# NttECS Architecture - Deep Analysis

## Core Philosophy

NttECS is a **high-performance, cache-friendly Entity Component System** designed for maximum throughput in real-time multiplayer games. The architecture prioritizes:
- **Zero-allocation hot paths** - All allocations happen during initialization
- **SIMD vectorization** - Hardware acceleration for search and iteration
- **Lock-free data structures** - Interlocked operations for thread safety
- **Packed component storage** - Struct-of-arrays layout for cache locality
- **Multi-threaded systems** - Automatic work distribution across CPU cores

## Architecture Components

### 1. NTT (Named Typed Thing) - server/NttECS/ECS/NTT.cs

The **lightweight entity handle** (16 bytes - just a Guid):
- Provides fluent API for component operations: `Set<T>()`, `Get<T>()`, `Has<T>()`, `Remove<T>()`
- All operations delegate to `PackedComponentStorage<T>` static methods
- Zero memory overhead per entity - just an identifier
- Readonly struct for immutability and performance
- Supports entity hierarchies via parent-child relationships

**Key Design:** NTT is just a handle, not a container. Components live in PackedComponentStorage.

### 2. PackedComponentStorage<T> - server/NttECS/ECS/PackedComponentStorage.cs

**High-performance component storage** using struct-of-arrays pattern:
- `T[] _components` - Dense array of components (contiguous memory)
- `Dictionary<Guid, int> _entityToIndex` - Entity → component index mapping
- `Guid[] _indexToEntity` - Component index → entity mapping
- `int _count` - Current component count

**Operations:**
- `AddFor()` - Add/update component with O(1) access
- `Get()` - Get mutable reference with O(1) access
- `Remove()` - Remove with swap-from-end compaction (maintains density)
- `GetComponentSpan()` - Get all components as ReadOnlySpan<T> for iteration
- `GetEntitySpan()` - Get all entity IDs parallel to component span

**Thread Safety:** Uses `ReaderWriterLockSlim` for concurrent access

**Cache Benefits:**
- Components stored contiguously enable CPU prefetching
- Systems iterate over dense arrays, not scattered objects
- SIMD operations can process multiple components per instruction

**Special Hook:** ParentChildComponent updates maintain NttWorld parent-child index

### 3. NttWorld - server/NttECS/ECS/NttWorld.cs

**Central ECS coordinator** managing entity lifecycle and system execution:

**Entity Management:**
- `Dictionary<Guid, NTT> NTTs` - All active entities
- `HashSet<NTT> Players` - Player entities subset
- `ConcurrentQueue<NTT> ToBeRemoved` - Deferred entity destruction
- `ConcurrentQueue<NTT> ChangedThisTick` - Entities with component changes

**System Coordination:**
- `NttSystem[] Systems` - Registered systems in execution order
- `UpdateSystems()` - Executes all systems in sequence (called by Game.GameLoop)
- `UpdateNTTs()` - Processes entity changes and destruction queues

**Parent-Child Indexing:**
- `Dictionary<NTT, List<NTT>> ParentToChildren` - O(1) child lookup
- Automatically maintained when ParentChildComponent added/removed
- `GetChildren(this NTT parent)` - Extension method for child access

**Tick Management:**
- `long Tick` - Current game tick counter
- `int TargetTps` - Target ticks per second (default: 60)
- `OnBeginTick` / `OnEndTick` - Callback hooks

**Key Flow:**
1. `OnBeginTick` callbacks
2. For each system:
   - Process entity change queue
   - Execute system.BeginUpdate()
3. Final entity change queue processing
4. `OnEndTick` callbacks
5. Increment tick counter

### 4. NttSystem - server/NttECS/ECS/NttSystem.cs

**Base class for all systems** with automatic entity filtering and multi-threading:

**Entity Tracking:**
- `ConcurrentDictionary<Guid, NTT> _entities` - Thread-safe filtered entities
- `SwapList<NTT> _entitiesList` - List view for efficient iteration
- `MatchesFilter(in NTT)` - Override to define component requirements

**Multi-Threading:**
- `int ThreadCount` - Number of threads for parallel processing
- `BeginUpdate()` - Decides single vs multi-threaded execution
- `EndUpdate()` - Calculates work distribution per thread
- Uses `ThreadedWorker.Run()` for parallel execution

**Generic Variants:** NttSystem<T>, NttSystem<T1,T2>, ... up to NttSystem<T1...T6>
- Automatic filtering via `Has<T1, T2, ...>()`
- Type-safe `Update(in NTT, ref T1, ref T2, ...)` abstract method
- Systems iterate `_entitiesList` and call `ntt.Get<T>()` for each component

**Threading Strategy:**
- Single-threaded: `_entitiesList.Count <= ThreadCount * 2`
- Multi-threaded: Work split evenly across threads with remainder distribution
- First `extraEntities` threads get +1 entity to process

### 5. SwapList<T> - server/NttECS/Memory/SwapList.cs

**Ultra-high performance list** with SIMD acceleration:

**Key Features:**
- **O(1) removal** via swap-with-last (order not preserved)
- **SIMD vectorized Contains() and IndexOf()** - 8x-16x speedup
- **Zero error handling** - maximum throughput, caller ensures validity
- **No allocations** during search operations

**SIMD Implementation:**
```csharp
if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported) {
    var searchVector = new Vector<T>(item);
    var vectorSize = Vector<T>.Count;
    for (i = 0; i <= _count - vectorSize; i += vectorSize) {
        var dataVector = new Vector<T>(_array, i);
        if (Vector.EqualsAny(dataVector, searchVector))
            return true; // Found in this vector
    }
    // Process remaining elements with scalar loop
}
```

**Use Cases:**
- System entity lists in NttSystem
- Any high-throughput collection with removals
- Works best with primitive numeric types (int, float, Guid)

**Performance Notes:**
- Vector<Guid> on AVX2 = 2 Guids per comparison (256 bits / 128 bits)
- Vector<int> on AVX2 = 8 ints per comparison
- Massive speedup for entity filtering and system updates

### 6. Pool<T> - server/NttECS/Memory/Pool.cs

**Lock-free thread-safe object pool** with zero runtime allocations:

**Design:**
- Pre-allocates all objects during construction
- Uses `Interlocked.Increment/Decrement` for thread-safe index management
- `Func<T> _factory` creates objects during initialization
- `Action<T> _reset` resets objects on return

**Operations:**
```csharp
public T Get() {
    Interlocked.Increment(ref _rentals);
    var idx = Interlocked.Decrement(ref _index);
    if (idx >= 0 && idx < _items.Length)
        return _items[idx]; // Pool hit
    Interlocked.Increment(ref _index);
    return _factory(); // Pool miss - create new
}

public void Return(T item) {
    _reset?.Invoke(item); // Reset state
    var idx = Interlocked.Increment(ref _index) - 1;
    if (idx >= 0 && idx < _items.Length)
        _items[idx] = item; // Return to pool
}
```

**Benefits:**
- No locks, no contentions - pure atomic operations
- Tracks rentals and returns for metrics
- Pool misses gracefully allocate new objects
- Static `Pool<T>.Shared` for common types

### 7. ThreadedWorker - server/NttECS/ECS/ThreadedWorker.cs

**High-performance thread pool** for parallel system execution:

**Architecture:**
- Creates `Environment.ProcessorCount` threads at startup
- Each thread set to `ThreadPriority.Highest`
- Uses `AutoResetEvent[]` for thread synchronization
- Threads sleep until signaled with work

**Execution Flow:**
```csharp
public static void Run(Action<int, int> action, int threads) {
    _numThreadsUsed = threads;
    Action = action;
    _allReady.Reset();
    Interlocked.Exchange(ref _readyThreads, 0);
    
    for (i = 0; i < threads; i++)
        _blocks[i].Set(); // Wake up thread
    
    _allReady.WaitOne(); // Wait for all threads to complete
}
```

**Thread Loop:**
```csharp
while (true) {
    _blocks[idx].WaitOne(); // Sleep until work assigned
    Action.Invoke(idx, _numThreadsUsed); // Execute work
    if (Interlocked.Increment(ref _readyThreads) == _numThreadsUsed)
        _allReady.Set(); // Last thread signals completion
}
```

**Benefits:**
- Zero thread creation overhead - threads persist
- Minimal context switching - threads sleep when idle
- Work stealing not needed - work pre-distributed
- Used by NttSystem for parallel entity processing

### 8. NttQuery - server/NttECS/ECS/NttQuery.cs

**Type-safe entity query system** for foreach iteration:

**Generic Enumerators:** QueryEnumerator<T>, QueryEnumerator<T1,T2>, ... up to 6 types

**Usage:**
```csharp
foreach (var ntt in NttQuery.Query<PositionComponent, VelocityComponent>()) {
    ref var pos = ref ntt.Get<PositionComponent>();
    ref var vel = ref ntt.Get<VelocityComponent>();
    // Process entity
}
```

**Implementation:**
- Wraps `Dictionary<Guid, NTT>.Enumerator`
- Filters entities during enumeration via `ntt.Has<T1, T2>()`
- Returns struct enumerator (no allocations)

**Use Cases:**
- One-off entity queries outside systems
- Debugging and inspection
- Utility functions

**Note:** Systems use pre-filtered entity lists for better performance

### 9. ReflectionHelper - server/NttECS/ECS/ReflectionHelper.cs

**Reflection-based component operations** across all component types:

**Initialization:**
- Scans all assemblies for types with `[Component]` attribute
- Pre-compiles delegates for `PackedComponentStorage<T>.Remove()`
- Caches delegates in `Dictionary<Type, Action<NTT, bool>>`

**Operations:**
- `Remove<T>(NTT)` - Remove specific component type
- `RecycleComponents(NTT)` - Remove all components (uses Parallel.ForEach)
- `LoadComponents(string)` - Load all components from disk in parallel

**Benefits:**
- Avoids runtime reflection cost - delegates cached at startup
- Enables generic operations across unknown component types
- Used during entity destruction and persistence

### 10. ComponentAttribute - server/NttECS/ECS/ComponentAttribute.cs

**Metadata attribute** for component configuration:

```csharp
[Component(ComponentType = ComponentType.Position, NetworkSync = true)]
public struct PositionComponent { ... }
```

**Properties:**
- `bool SaveEnabled` - Should component persist to disk?
- `ComponentType ComponentType` - Network packet type enum
- `bool NetworkSync` - Should sync to clients?

**Usage:**
- ReflectionHelper discovers components via this attribute
- Game.cs uses ComponentType for packet serialization
- Save system uses SaveEnabled for persistence

### 11. MultiThreadWorkQueue<T> - server/NttECS/Threading/MultiThreadWorkQueue.cs

**Producer-consumer work queue** for background processing:

**Architecture:**
- `BlockingCollection<T> Queue` - Thread-safe queue
- `Thread[] workerThreads` - Worker thread pool
- `Action<T> OnExec` - Work item processor

**Usage:**
```csharp
var queue = new MultiThreadWorkQueue<Packet>(ProcessPacket, threadCount: 4);
queue.Enqueue(packet); // Producer
queue.Stop(); // Graceful shutdown
```

**Use Cases:**
- Packet processing in network systems
- Background I/O operations
- Async work distribution

## Performance Patterns

### 1. Cache Locality
- Components stored in dense arrays, not scattered objects
- Systems iterate contiguous memory for cache hits
- SIMD operations benefit from aligned data

### 2. Lock-Free Concurrency
- Pool<T> uses Interlocked atomic operations
- ConcurrentQueue for entity changes
- Minimal lock contention

### 3. SIMD Vectorization
- SwapList<T> vectorizes Contains/IndexOf
- 8x-16x speedup on primitive types
- Hardware acceleration always enabled

### 4. Zero Allocation Hot Paths
- All allocations during initialization
- No GC pressure during game loop
- Object pooling for dynamic allocations

### 5. Multi-Threading
- Systems automatically partition work
- ThreadedWorker manages thread pool
- High priority threads minimize latency

### 6. Struct-of-Arrays (SoA)
- PackedComponentStorage stores components separately
- Enables efficient iteration over single component type
- Better than Array-of-Structs (AoS) for cache efficiency

## System Execution Model

### Game Loop (server/Simulation/Game.cs)
1. Game.GameLoop calls NttWorld.UpdateSystems()
2. NttWorld.UpdateSystems():
   - Calls OnBeginTick callbacks
   - For each system in Systems[]:
     - Processes entity change queue (UpdateNTTs)
     - Calls system.BeginUpdate(deltaTime)
   - Final UpdateNTTs
   - Calls OnEndTick callbacks
   - Increments Tick counter

### System Execution (NttSystem.BeginUpdate)
1. Check if entity list is empty (early return)
2. Decide threading:
   - Multi-threaded: `ThreadCount > 1 && _entitiesList.Count > ThreadCount * 2`
   - Single-threaded: Otherwise
3. Multi-threaded path:
   - Call ThreadedWorker.Run(EndUpdate, ThreadCount)
   - EndUpdate calculates work distribution
   - Each thread calls Update(start, amount)
4. Single-threaded path:
   - Directly call Update(0, _entitiesList.Count)

### Entity Filtering (NttSystem.EntityChanged)
1. Entity components change (Set/Remove)
2. NttWorld.InformChangesFor(ntt) enqueues entity
3. During system updates, NttWorld.UpdateNTTs() processes queue
4. For each system:
   - system.EntityChanged(ntt)
   - Evaluates MatchesFilter(ntt)
   - Adds/removes from system's entity collection

## Advanced Features

### Parent-Child Hierarchies
- ParentChildComponent special handling in PackedComponentStorage
- NttWorld maintains ParentToChildren index
- O(1) child lookup via GetChildren() extension
- Automatic index updates on component add/remove/change

### Network Synchronization
- Components marked with NetworkSync = true
- ComponentSyncSystem serializes changed components
- Only changed entities sent to clients (ChangedThisTick queue)
- Viewport culling reduces network traffic

### Persistence
- Components marked with SaveEnabled = true
- ReflectionHelper.LoadComponents() at startup
- Parallel load/save for performance
- JSON serialization via System.Text.Json

## Comparison to Other ECS Frameworks

### vs Unity ECS (DOTS)
- **Similarities:** Packed component storage, SIMD, multi-threading
- **Differences:** 
  - NttECS: Simpler API, manual system ordering
  - Unity: Automatic dependency resolution, job system

### vs Entitas
- **Similarities:** Component-based filtering, system iteration
- **Differences:**
  - NttECS: Packed storage, SIMD, lock-free
  - Entitas: Code generation, reactive systems

### vs Flecs
- **Similarities:** Query-based iteration, parent-child relationships
- **Differences:**
  - NttECS: Static type safety, C# specific
  - Flecs: Dynamic types, C-based, more flexible

## Strengths

1. **Extreme Performance** - SIMD, packed storage, lock-free algorithms
2. **Type Safety** - Generic systems, compile-time filtering
3. **Simple API** - Fluent entity operations, minimal boilerplate
4. **Multi-Threading** - Automatic work distribution
5. **Cache Friendly** - Contiguous component storage
6. **Zero Allocations** - All allocations during initialization

## Weaknesses / Trade-offs

1. **Manual System Ordering** - Developer must order systems correctly
2. **No Reactive Systems** - No automatic event listeners
3. **Limited Query Features** - No excludes, no optional components
4. **C# Specific** - Not portable to other languages
5. **Manual Threading** - No automatic dependency analysis
6. **Type Erasure Limitations** - ReflectionHelper needed for cross-type ops

## Future Optimization Opportunities (TODOs in code)

1. **Multi-Component Packed Iteration** (NttSystem.cs:148)
   - Currently: Systems iterate entities, call Get<T>() per component
   - Future: Iterate multiple PackedComponentStorage arrays in parallel
   - Benefit: Even better cache locality, reduced indirection

2. **Query Excludes** - Add not<T>() filtering
3. **Optional Components** - Add maybe<T>() support
4. **Change Detection** - Track component modifications per tick
5. **Archetype Optimization** - Group entities by component signature

## Key Insights

### Why Packed Storage?
Traditional ECS: Entity stores List<Component> → pointer chasing, cache misses
NttECS: Components in separate T[] arrays → sequential access, cache hits

### Why SIMD?
Linear search through 1000 entities: 1000 comparisons
SIMD (AVX2 int): 8 ints per comparison → 125 vector ops
Speedup: 8x theoretical, 6-10x practical (depends on data)

### Why Lock-Free?
Locks cause thread contention → context switches → performance loss
Interlocked operations: Hardware atomic instructions, no context switches
Result: 10-100x faster for high-contention scenarios

### Why Swap-Remove?
Traditional remove: Shift all elements → O(n)
Swap-remove: Move last element to gap → O(1)
Trade-off: Element order not preserved (acceptable for entity lists)

### Why Multi-Threading?
Modern CPUs: 8-16+ cores, single-threaded wastes resources
NttECS: Automatically distributes entity processing across cores
Result: Near-linear scaling with core count

## Recommended Usage Patterns

### Creating Entities
```csharp
ref var ntt = ref NttWorld.CreateEntity();
ntt.Set(new PositionComponent { X = 0, Y = 0 });
ntt.Set(new VelocityComponent { X = 1, Y = 1 });
```

### Accessing Components
```csharp
// Systems: ref for mutation
ref var pos = ref ntt.Get<PositionComponent>();
pos.X += velocity.X * deltaTime;

// Read-only: in parameter
if (ntt.Has<PositionComponent>()) {
    var pos = ntt.Get<PositionComponent>();
    Console.WriteLine($"Position: {pos.X}, {pos.Y}");
}
```

### Creating Systems
```csharp
public class MovementSystem : NttSystem<PositionComponent, VelocityComponent>
{
    public MovementSystem() : base("Movement", threads: 4) { }
    
    public override void Update(in NTT ntt, ref PositionComponent pos, ref VelocityComponent vel)
    {
        pos.X += vel.X * DeltaTime;
        pos.Y += vel.Y * DeltaTime;
    }
}
```

### Registering Systems
```csharp
NttWorld.SetSystems(
    new SpawnSystem(),
    new InputSystem(),
    new MovementSystem(),
    new CollisionSystem(),
    new RenderSystem()
);
```

## Conclusion

NttECS is a **production-ready, high-performance ECS framework** optimized for real-time multiplayer games. The architecture demonstrates expert-level C# optimization techniques including SIMD vectorization, lock-free algorithms, packed storage, and automatic multi-threading. The codebase shows deep understanding of CPU cache behavior, memory layout, and modern hardware capabilities.

The framework makes deliberate trade-offs (manual system ordering, limited queries) in exchange for maximum performance and simplicity. It's ideal for games requiring high entity counts (1000s-10000s) with complex multi-threaded physics and simulation.