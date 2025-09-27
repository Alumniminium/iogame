**Complete ECS Ship Part System Plan**

## 1. Core Architecture

**Ship parts are full entities using existing components. No special part components.**

```
Player Ship Entity (NTT)
├── PlayerTagComponent (identifies main player)
├── EngineComponent (main ship systems)
├── WeaponComponent
├── ShieldComponent
├── EnergyComponent
├── ShipAssemblyComponent (tracks part entities)
└── Part Entities:
    ├── Engine Part Entity (NTT)
    │   ├── EngineComponent (individual engine)
    │   ├── EnergyComponent (local power)
    │   ├── GridPositionComponent
    │   ├── ChildOffsetComponent (parent = ship)
    │   └── NetSyncComponent(SyncThings.All)
    ├── Weapon Part Entity (NTT)
    │   ├── WeaponComponent (individual weapon)
    │   ├── GridPositionComponent
    │   ├── ChildOffsetComponent
    │   └── NetSyncComponent(SyncThings.All)
    └── Shield Part Entity (NTT)
        ├── ShieldComponent (individual shield)
        ├── GridPositionComponent
        ├── ChildOffsetComponent
        └── NetSyncComponent(SyncThings.All)
```

## 2. New Components

**PlayerTagComponent**
```csharp
[Component]
public struct PlayerTagComponent
{
    public NTT EntityId;
    public long ChangedTick;
}
```

**ShipAssemblyComponent**
```csharp
[Component]
public struct ShipAssemblyComponent
{
    public NTT EntityId;
    public List<NTT> PartEntities;
    public sbyte CenterX;
    public sbyte CenterY;
    public long ChangedTick;
}
```

**ComponentTypeFlags** (for network sync)
```csharp
[Flags]
public enum ComponentTypeFlags : uint
{
    None = 0,
    Engine = 1,
    Shield = 2,
    Weapon = 4,
    Energy = 8,
    Health = 16,
    GridPosition = 32,
    ChildOffset = 64,
    // Add more as needed
}
```

## 3. Cached Component Flags in NTT

**Add to NTT struct:**
```csharp
public readonly struct NTT(Guid id)
{
    public readonly Guid Id = id;

    // CACHE component flags here
    public ComponentTypeFlags ComponentFlags { get; private set; } = ComponentTypeFlags.None;

    // Updated Set methods
    public readonly void Set<T>(ref T t) where T : struct
    {
        PackedComponentStorage<T>.AddFor(in this, ref t);
        UpdateComponentFlags<T>(true); // Add flag
        NttWorld.InformChangesFor(this);
    }

    public readonly void Set<T>(T component) where T : struct
    {
        PackedComponentStorage<T>.AddFor(in this, ref component);
        UpdateComponentFlags<T>(true); // Add flag
        NttWorld.InformChangesFor(this);
    }

    public readonly void Set<T>() where T : struct
    {
        PackedComponentStorage<T>.AddFor(in this);
        UpdateComponentFlags<T>(true); // Add flag
        NttWorld.InformChangesFor(this);
    }

    // Updated Remove method
    public readonly void Remove<T>() where T : struct
    {
        ReflectionHelper.Remove<T>(this);
        UpdateComponentFlags<T>(false); // Remove flag
        NttWorld.InformChangesFor(this);
    }

    // Flag update logic
    private readonly void UpdateComponentFlags<T>(bool add) where T : struct
    {
        var flag = GetComponentFlag<T>();
        if (add)
            ComponentFlags |= flag;
        else
            ComponentFlags &= ~flag;
    }

    private static ComponentTypeFlags GetComponentFlag<T>() where T : struct
    {
        return typeof(T).Name switch
        {
            nameof(EngineComponent) => ComponentTypeFlags.Engine,
            nameof(ShieldComponent) => ComponentTypeFlags.Shield,
            nameof(WeaponComponent) => ComponentTypeFlags.Weapon,
            nameof(EnergyComponent) => ComponentTypeFlags.Energy,
            nameof(HealthComponent) => ComponentTypeFlags.Health,
            nameof(GridPositionComponent) => ComponentTypeFlags.GridPosition,
            nameof(ChildOffsetComponent) => ComponentTypeFlags.ChildOffset,
            _ => ComponentTypeFlags.None
        };
    }
}
```

## 4. Part Entity Creation

**Manual entity assembly in PacketHandler:**
```csharp
// In PacketHandler.cs ship configuration:
foreach(var part in packet.Parts)
{
    var partEntity = NttWorld.CreateEntity();

    // CORE: All parts have positioning
    var gridPos = new GridPositionComponent
    {
        GridPos = new Vector2Int(part.GridX, part.GridY),
        Assembly = player,
        Rotation = (BlockRotation)part.Rotation
    };

    var childOffset = new ChildOffsetComponent(partEntity, player.Id,
        new Vector2(part.GridX, part.GridY), part.Rotation * MathF.PI / 2f);

    var sync = new NetSyncComponent(partEntity, SyncThings.All);

    // CLIENT DETERMINES COMPONENTS: Use existing ShipPart.Type for backwards compatibility
    switch(part.Type)
    {
        case 0: // Hull
            var health = new HealthComponent(partEntity, 100, 100);
            partEntity.Set(ref health);
            break;

        case 1: // Shield
            var shield = new ShieldComponent(partEntity, 100f, 100, 5f, 10f, 5f, 10f, TimeSpan.FromSeconds(3));
            var shieldEnergy = new EnergyComponent(partEntity, 1000, 2000, 5000);
            partEntity.Set(ref shield);
            partEntity.Set(ref shieldEnergy);
            break;

        case 2: // Engine
            var engine = new EngineComponent(partEntity, 25f);
            var engineEnergy = new EnergyComponent(partEntity, 500, 1000, 2000);
            partEntity.Set(ref engine);
            partEntity.Set(ref engineEnergy);
            break;

        case 3: // Weapon
            var weapon = new WeaponComponent(partEntity, 0f, 5, 1, 1, 30, 50, TimeSpan.FromMilliseconds(350));
            var weaponEnergy = new EnergyComponent(partEntity, 300, 500, 1000);
            partEntity.Set(ref weapon);
            partEntity.Set(ref weaponEnergy);
            break;
    }

    partEntity.Set(ref gridPos);
    partEntity.Set(ref childOffset);
    partEntity.Set(ref sync);

    partEntities.Add(partEntity);
}
```

## 5. Network Protocol

**PartEntitySpawnPacket:**
```csharp
public class PartEntitySpawnPacket
{
    public NTT PartEntityId;
    public Vector2Int GridPos;
    public BlockRotation Rotation;
    public ComponentTypeFlags ComponentTypes; // Cached flags!
}

public class ShipAssemblyPacket
{
    public NTT ShipEntityId;
    public List<PartEntitySpawnPacket> Parts;
}
```

**Server spawn logic:**
```csharp
// In PacketHandler.cs ship configuration:
case PacketId.ShipConfiguration:
{
    var packet = ShipConfigurationPacket.FromBuffer(buffer);

    // Create part entities
    var partEntities = new List<NTT>();
    var partSpawnPackets = new List<PartEntitySpawnPacket>();

    // Manual entity assembly (see section 4)
    foreach(var part in packet.Parts)
    {
        // ... entity creation code from section 4 ...

        // Use CACHED component flags
        var spawnPacket = new PartEntitySpawnPacket
        {
            PartEntityId = partEntity,
            GridPos = new Vector2Int(part.GridX, part.GridY),
            Rotation = (BlockRotation)part.Rotation,
            ComponentTypes = partEntity.ComponentFlags // CACHED!
        };
        partSpawnPackets.Add(spawnPacket);
    }

    // Replace old component
    if (player.Has<ShipConfigurationComponent>())
        player.Remove<ShipConfigurationComponent>();

    var assembly = new ShipAssemblyComponent(player, partEntities, packet.CenterX, packet.CenterY);
    player.Set(ref assembly);

    // Send to clients
    var assemblyPacket = new ShipAssemblyPacket
    {
        ShipEntityId = player,
        Parts = partSpawnPackets
    };
    Game.Broadcast(assemblyPacket.ToBuffer());
    break;
}
```

**Client spawn logic:**
```csharp
// Client packet handler:
window.addEventListener("ship-assembly", (event) => {
    const { shipEntityId, parts } = event.detail;

    for (const partData of parts) {
        const partEntity = World.getOrCreateEntity(partData.partEntityId);

        // Create empty components based on flags
        if (partData.componentTypes & ComponentTypeFlags.Engine) {
            partEntity.addComponent(new EngineComponent());
        }
        if (partData.componentTypes & ComponentTypeFlags.Shield) {
            partEntity.addComponent(new ShieldComponent());
        }
        if (partData.componentTypes & ComponentTypeFlags.Energy) {
            partEntity.addComponent(new EnergyComponent());
        }
        if (partData.componentTypes & ComponentTypeFlags.Health) {
            partEntity.addComponent(new HealthComponent());
        }
        // Always add positioning
        partEntity.addComponent(new GridPositionComponent());
        partEntity.addComponent(new ChildOffsetComponent());

        // NetSyncSystem will immediately sync actual component data
    }
});
```

## 6. System Updates

**Input System (Player-only):**
```csharp
public sealed class InputSystem : NttSystem<PlayerTagComponent, InputComponent, EngineComponent>
{
    public override void Update(in NTT ntt, ref PlayerTagComponent player, ref InputComponent input, ref EngineComponent eng)
    {
        // Only main player entities get input processing
    }
}
```

**Engine System (All engines):**
```csharp
public sealed class Box2DEngineSystem : NttSystem<Box2DBodyComponent, EngineComponent, EnergyComponent>
{
    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref EnergyComponent nrg)
    {
        // Processes BOTH player engines AND part engines automatically!
    }
}
```

## 7. Implementation Phases

**Phase 1: Component Infrastructure**
1. Add PlayerTagComponent, ShipAssemblyComponent
2. Add ComponentTypeFlags enum
3. Update NTT.Set() and Remove() methods with flag caching

**Phase 2: Network Protocol**
1. Create PartEntitySpawnPacket, ShipAssemblyPacket
2. Update PacketHandler.cs ship configuration case
3. Replace ShipConfigurationComponent with ShipAssemblyComponent
4. Send part spawn packets with cached component flags

**Phase 3: System Updates**
1. Add PlayerTagComponent filter to input systems
2. Remove player-specific filters from component systems
3. Update client packet handlers to create component slots
4. Test part entity sync via existing NetSyncSystem

## 8. Key Benefits

1. **No Re-computation**: Component flags cached in NTT Set/Remove
2. **Minimal Network Data**: Only flags sent, NetSyncSystem handles data
3. **Component Reuse**: Parts use same components as players
4. **System Reuse**: Existing systems work on parts automatically
5. **Custom Combinations**: Players can mix any components on parts
6. **Architecture Consistency**: Parts are entities, no special cases

## 9. Implementation Checklist

- [ ] Create ComponentTypeFlags enum in server/Simulation/Components/
- [ ] Create PlayerTagComponent struct in server/Simulation/Components/
- [ ] Create ShipAssemblyComponent to replace ShipConfigurationComponent
- [ ] Add ComponentFlags property to NTT struct
- [ ] Update NTT.Set() methods to cache component flags
- [ ] Update NTT.Remove() method to update component flags
- [ ] Modify PacketHandler ShipConfiguration case to create part entities
- [ ] Update PacketHandler to use ShipAssemblyComponent
- [ ] Add PlayerTagComponent filter to InputSystem
- [ ] Create PartEntitySpawnPacket and ShipAssemblyPacket
- [ ] Update server to send part spawn packets
- [ ] Create client-side PlayerTagComponent
- [ ] Create client-side ShipAssemblyComponent
- [ ] Update client PacketHandler for part entity spawning
- [ ] Test and verify implementation
- [ ] Update CHANGELOG.md