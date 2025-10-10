# Common Development Patterns

## Adding New Components

1. Create data-only struct in `Simulation/Components/`:
```csharp
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.YourComponent, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct YourComponent(float value)
{
    /// MUST be first field for raw byte access in ComponentSerializer
    public long ChangedTick = NttWorld.Tick;
    
    public float Value = value;
}
```

2. Add component type to `Enums/ComponentIds.cs`:
```csharp
public enum ComponentType
{
    // ... existing types
    YourComponent,
}
```

3. If NetworkSync = true and client needs it:
   - Add TypeScript component class in pixiejsClient
   - Update ComponentType enum in client
   - Add deserialization case in ComponentStatePacket.ts

## Adding New Systems

1. Create system in `Simulation/Systems/`:
```csharp
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Description of what this system does.
/// </summary>
public sealed class YourSystem : NttSystem<Component1, Component2>
{
    public YourSystem() : base("Your System", threads: 1) { }
    
    public override void Update(in NTT ntt, ref Component1 c1, ref Component2 c2)
    {
        // System logic
        
        // Mark components as changed if modified
        if (dataChanged)
        {
            c1.ChangedTick = NttWorld.Tick;
        }
    }
}
```

2. Register in `Simulation/Game.cs` constructor:
```csharp
systems = 
[
    // ... existing systems
    new YourSystem(),  // Add in correct order!
];
```

**CRITICAL**: System order matters! Place carefully in execution order.

## Adding New Packet Types

1. Define packet class in pixiejsClient:
```typescript
export class YourPacket {
    static write(writer: EvPacketWriter, data: SomeData) {
        // Serialize data
    }
    
    static read(reader: EvPacketReader): SomeData {
        // Deserialize data
    }
}
```

2. Add to `PacketId` enum in client `PacketIds.ts`

3. Add handler in `server/Simulation/Net/PacketHandler.cs`:
```csharp
case PacketId.YourPacket:
    HandleYourPacket(player, reader);
    break;
```

4. Add client-side handling in NetworkManager or appropriate system

## Entity Spawning

Use `SpawnManager.cs` for consistent entity creation:
```csharp
var entity = SpawnManager.SpawnEntity(world, position, components...);
```

SpawnManager ensures:
- Proper component initialization
- Network sync setup
- Consistent entity configuration

## Marking Components as Changed

Components are only synced when changed:
```csharp
// Modify component data
component.Health -= damage;

// Mark as changed for network sync
component.ChangedTick = NttWorld.Tick;
```

## Multi-Threading Considerations

Systems with `threads > 1`:
- Entities automatically partitioned across threads
- Ensure thread-safe operations
- Avoid shared mutable state
- Use lock-free patterns (Interlocked, Pool<T>)

## Performance Best Practices

- **Use struct components** - value types for cache efficiency
- **Avoid allocations in hot paths** - use pooling (Pool<T>)
- **Mark systems multi-threaded** when possible - `threads > 1`
- **Use ref parameters** - avoid copying structs
- **Profile before optimizing** - use PerformanceMetrics helpers