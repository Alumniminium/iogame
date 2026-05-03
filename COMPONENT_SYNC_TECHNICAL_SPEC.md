# ComponentSync System Technical Specification

**IOGame Network Synchronization Protocol**

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Architecture Overview](#architecture-overview)
3. [Server-Side Implementation](#server-side-implementation)
4. [Binary Protocol Specification](#binary-protocol-specification)
5. [Client-Side Implementation](#client-side-implementation)
6. [Component Mapping Reference](#component-mapping-reference)
7. [Dirty Tracking Mechanism](#dirty-tracking-mechanism)
8. [Viewport Culling Integration](#viewport-culling-integration)
9. [Packet Batching & Network Layer](#packet-batching--network-layer)
10. [Implementation Examples](#implementation-examples)
11. [Performance Characteristics](#performance-characteristics)

---

## Executive Summary

The ComponentSync system is a **reflection-based, binary serialization framework** that synchronizes ECS component state from the .NET server to TypeScript/PixiJS clients over WebSocket. Key design principles:

- **Selective Sync**: Only components marked `NetworkSync = true` are transmitted
- **Dirty Tracking**: Components sync only when `ChangedTick == CurrentTick`
- **Viewport Culling**: Only entities visible to a player are synchronized
- **Binary Protocol**: Compact struct serialization with deterministic layout
- **Batched Transmission**: Multiple packets concatenated into single WebSocket frames

**Data Flow**:
```
Server Systems → Modify Components → Set ChangedTick → ComponentSyncSystem →
ComponentSerializer → PacketWriter → PacketQueue → WebSocket →
Client → PacketHandler → ComponentStatePacket → Component.fromBuffer() → Entity
```

---

## Architecture Overview

### System Participants

| Layer | Server (C#) | Client (TypeScript) |
|-------|-------------|---------------------|
| **Sync System** | `ComponentSyncSystem` | N/A (receive-only) |
| **Serializer** | `ComponentSerializer` | `Component.fromBuffer()` |
| **Packet Writer** | `PacketWriter` | `EvPacketWriter` |
| **Packet Reader** | N/A | `EvPacketReader` |
| **Packet Handler** | `PacketHandler` (incoming) | `PacketHandler` |
| **Queue/Transport** | `PacketQueue` → WebSocket | WebSocket → Queue |
| **Packet Type** | `ComponentStatePacket` | `ComponentStatePacket` |

### Execution Order in Game Loop

The `ComponentSyncSystem` executes near the end of the tick, after all game systems have modified components:

```
Game.cs systems list:
1.  SpawnSystem
2.  ViewportSystem          ← Populates EntitiesVisible
3.  InputSystem
4.  PositionSyncSystem
5.  ShipPhysicsRebuildSystem
6.  GravitySystem
7.  EngineSystem
8.  EnergySystem
9.  ShieldSystem
10. WeaponSystem
11. PickupCollisionResolver
12. ProjectileCollisionSystem
13. DamageSystem
14. HealthSystem
15. DropSystem
16. LifetimeSystem
17. LevelExpSystem
18. RespawnSystem
19. ComponentSyncSystem     ← Serializes changed components
20. DeathSystem
```

After all systems run, `PacketQueue.FlushAll()` batches and sends packets.

---

## Server-Side Implementation

### ComponentSyncSystem

**File**: `server/Simulation/Systems/ComponentSyncSystem.cs`

```csharp
public sealed class ComponentSyncSystem : NttSystem<NetworkComponent, ViewportComponent>
{
    public ComponentSyncSystem() : base("Component Sync System", threads: 1) { }

    public override void Update(in NTT ntt, ref NetworkComponent network, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        // Sync all entities visible to this player
        foreach (var visibleEntity in vwp.EntitiesVisible)
        {
            SyncEntity(ntt, visibleEntity);
            SyncChildEntities(ntt, visibleEntity);
        }

        // Always sync player's own entity (even if not in viewport list)
        SyncEntity(ntt, ntt);
        SyncChildEntities(ntt, ntt);
    }

    private static void SyncEntity(NTT viewer, NTT entity)
    {
        foreach (var componentType in ComponentSerializer.NetworkSyncTypes)
            ComponentSerializer.TrySyncComponent(viewer, entity, componentType);
    }

    private static void SyncChildEntities(NTT viewer, NTT parentEntity)
    {
        foreach (var child in parentEntity.GetChildren())
            SyncEntity(viewer, child);
    }
}
```

**Key Behaviors**:
- Iterates `ViewportComponent.EntitiesVisible` (populated by `ViewportSystem`)
- Recursively syncs child entities (ship parts attached to parent)
- Always syncs the viewer's own entity regardless of viewport

### ComponentSerializer

**File**: `server/Serialization/ComponentSerializer.cs`

```csharp
public static class ComponentSerializer
{
    private static readonly Dictionary<Type, byte> _componentIds = [];
    private static readonly Dictionary<Type, bool> _netSyncTypes = [];

    // Cached list of types where NetworkSync = true
    public static IEnumerable<Type> NetworkSyncTypes =>
        _netSyncTypes.Where(kvp => kvp.Value).Select(kvp => kvp.Key);

    static ComponentSerializer()
    {
        // Cache component metadata at startup via reflection
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var attr = type.GetCustomAttribute<ComponentAttribute>();
            if (attr != null && attr.ComponentType != ComponentType.None)
            {
                _componentIds[type] = (byte)attr.ComponentType;
                _netSyncTypes[type] = attr.NetworkSync;
            }
        }
    }

    public static Memory<byte> Serialize<T>(NTT entity, ref T component) where T : struct
    {
        if (!_componentIds.TryGetValue(typeof(T), out var componentId))
            throw new InvalidOperationException($"Component {typeof(T).Name} not registered");

        using var writer = new PacketWriter(PacketId.ComponentState);
        writer.WriteNtt(entity);           // 16 bytes (GUID)
        writer.WriteByte(componentId);     // 1 byte

        // Serialize entire struct as raw bytes
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref component, 1));
        writer.WriteInt16((short)bytes.Length);  // 2 bytes
        writer.WriteBytes(bytes.ToArray());      // N bytes (struct size)

        return writer.Finalize();
    }

    public static void TrySyncComponent(NTT viewer, NTT entity, Type componentType)
    {
        // Invoke generic TrySyncTyped<T> via reflection
        var method = typeof(ComponentSerializer).GetMethod(nameof(TrySyncTyped),
            BindingFlags.NonPublic | BindingFlags.Static);
        var genericMethod = method!.MakeGenericMethod(componentType);
        genericMethod.Invoke(null, [viewer, entity]);
    }

    private static void TrySyncTyped<T>(NTT viewer, NTT entity) where T : struct
    {
        if (!entity.Has<T>()) return;

        ref var component = ref entity.Get<T>();

        // Skip InputComponent (client → server only)
        if (typeof(T) == typeof(InputComponent))
            return;

        // Read ChangedTick from first 8 bytes of struct (raw byte access)
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref component, 1));
        var changedTick = MemoryMarshal.Read<long>(bytes);

        // Only sync if component changed THIS tick
        if (changedTick == NttWorld.Tick)
            viewer.NetSync(Serialize(entity, ref component));
    }
}
```

**Critical Design Decisions**:

1. **Raw Byte Serialization**: Uses `MemoryMarshal.AsBytes()` to treat structs as byte arrays. No per-field serialization—entire struct is copied.

2. **ChangedTick at Offset 0**: The dirty check reads `ChangedTick` directly from raw bytes at offset 0. This requires all components to have `ChangedTick` as their **first field**.

3. **Reflection at Startup Only**: Component metadata is cached in the static constructor. Runtime uses only cached data.

4. **InputComponent Exclusion**: Input flows client → server, never server → client.

### PacketWriter

**File**: `server/Simulation/Net/PacketWriter.cs`

```csharp
public class PacketWriter : IDisposable
{
    private byte[] _buffer;
    private int _offset;
    private readonly bool _fromPool;

    public PacketWriter(PacketId packetId, int initialSize = 4096)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(initialSize);
        _offset = 0;

        WriteInt16(0);                    // Placeholder for packet size
        WriteInt16((short)packetId);      // Packet type ID
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteGuid(Guid guid)
    {
        EnsureCapacity(16);
        guid.TryWriteBytes(_buffer.AsSpan(_offset));
        _offset += 16;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteInt16(short value)
    {
        EnsureCapacity(2);
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 2;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteBytes(ReadOnlySpan<byte> bytes)
    {
        EnsureCapacity(bytes.Length);
        bytes.CopyTo(_buffer.AsSpan(_offset));
        _offset += bytes.Length;
        return this;
    }

    public Memory<byte> Finalize()
    {
        // Backfill packet size at offset 0
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(0), (short)_offset);

        var result = new byte[_offset];
        _buffer.AsSpan(0, _offset).CopyTo(result);
        return result;
    }

    public void Dispose()
    {
        if (_fromPool && _buffer != null)
            ArrayPool<byte>.Shared.Return(_buffer);
    }
}
```

**Performance Features**:
- `ArrayPool<byte>.Shared` for zero-allocation buffer reuse
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for hot path optimization
- `BinaryPrimitives` for platform-agnostic little-endian encoding

---

## Binary Protocol Specification

### Packet Frame Structure

All packets share this header structure:

```
Offset  Size  Type   Field         Description
──────  ────  ────   ─────         ───────────
0       2     i16    PacketSize    Total packet length (includes header)
2       2     i16    PacketId      Packet type identifier
4       N     bytes  Payload       Packet-specific data
```

**Byte Order**: Little-endian for all multi-byte values.

### ComponentStatePacket Structure

**PacketId**: `50`

```
Offset  Size  Type   Field           Description
──────  ────  ────   ─────           ───────────
0       2     i16    PacketSize      Total packet size
2       2     i16    PacketId        = 50 (ComponentState)
4       16    GUID   EntityId        NTT.Id of target entity
20      1     u8     ComponentType   ComponentType enum value
21      2     i16    DataLength      Length of component data
23      N     bytes  ComponentData   Raw struct bytes
```

**Total Size**: 23 + struct_size bytes

### GUID Serialization Format

.NET GUIDs serialize in a specific mixed-endian format:

```
GUID: "12345678-abcd-ef01-2345-6789abcdef01"

Byte Layout (16 bytes):
[0-3]   Data1 (i32, little-endian): 78 56 34 12
[4-5]   Data2 (i16, little-endian): cd ab
[6-7]   Data3 (i16, little-endian): 01 ef
[8-9]   Data4[0-1] (big-endian):    23 45
[10-15] Data4[2-7] (big-endian):    67 89 ab cd ef 01
```

### PacketId Enum Values

```csharp
public enum PacketId : short
{
    LoginRequest = 1,
    LoginResponse = 2,
    ChatPacket = 10,
    InputPacket = 21,
    PresetSpawnPacket = 30,
    CustomSpawnPacket = 31,
    LineSpawnPacket = 33,
    ComponentState = 50,
    Ping = 90,
}
```

### ComponentType Enum Values

**File**: `server/Enums/ComponentIds.cs`

```csharp
public enum ComponentType : byte
{
    None = 0,
    Box2DBody = 1,    // Physics
    Gravity = 2,
    Health = 3,
    Shield = 4,
    Energy = 5,
    Engine = 6,
    Level = 8,
    Inventory = 9,
    Viewport = 10,
    NameTag = 11,
    DeathTag = 12,
    ShipPart = 13,
    ParentChild = 14,
    Color = 15,
    Lifetime = 16,
    Input = 17,
    HealthRegen = 18,
    BodyDamage = 19,
    Bullet = 20,
    Collision = 21,
    Damage = 22,
    ExpReward = 23,
    SlaveOffset = 24,
    Network = 25,
    Asteroid = 26,
    Pickup = 27,
    RespawnTag = 28,
    Spawner = 29,
    Weapon = 30,
    // ... more types
}
```

---

## Client-Side Implementation

### PacketHandler

**File**: `pixiejsClient/src/app/network/PacketHandler.ts`

```typescript
export class PacketHandler {
  private packetQueue: ArrayBuffer[] = [];

  queuePacket(data: ArrayBuffer): void {
    this.packetQueue.push(data);
  }

  processQueuedPackets(): void {
    const packetsToProcess = [...this.packetQueue];
    this.packetQueue.length = 0;

    for (const data of packetsToProcess) {
      this.processPacket(data);
    }
  }

  private processPacket(data: ArrayBuffer): void {
    const view = new DataView(data);
    let offset = 0;

    // Handle multiple concatenated packets in one WebSocket message
    while (offset < data.byteLength) {
      if (offset + 4 > data.byteLength) break;

      const packetLength = view.getUint16(offset, true);
      const packetId = view.getUint16(offset + 2, true) as PacketId;

      // Validate packet
      if (packetLength < 4 || packetLength > 65535) {
        // Recovery logic...
        break;
      }

      const packetData = view.buffer.slice(offset) as ArrayBuffer;

      switch (packetId) {
        case PacketId.ComponentState:
          ComponentStatePacket.handle(packetData);
          break;
        // ... other packet types
      }

      offset += packetLength;
    }
  }
}
```

**Key Feature**: Handles **batched packets** by iterating through concatenated packet frames within a single WebSocket message.

### ComponentStatePacket Handler

**File**: `pixiejsClient/src/app/network/packets/ComponentStatePacket.ts`

```typescript
export class ComponentStatePacket {
  header: PacketHeader;
  ntt: NTT;
  componentId: number;
  dataLength: number;
  data: ArrayBuffer;

  static handle(buffer: ArrayBuffer) {
    const packet = ComponentStatePacket.fromBuffer(buffer);

    // Get or create entity
    let entity = World.getEntity(packet.ntt.id);
    if (!entity) {
      entity = World.createEntity(packet.ntt.id);
    }

    // Special case: NameTag has custom handling
    if (packet.componentId === ServerComponentType.NameTag) {
      // Custom deserialization for fixed-length string
      const reader = new EvPacketReader(packet.data);
      reader.i64(); // Skip changedTick
      const nameBytes = new Uint8Array(64);
      for (let i = 0; i < 64; i++) {
        nameBytes[i] = reader.i8();
      }
      const nullIndex = nameBytes.indexOf(0);
      const nameString = new TextDecoder().decode(
        nameBytes.subarray(0, nullIndex >= 0 ? nullIndex : 64)
      );
      PlayerNameManager.getInstance().setPlayerName(packet.ntt.id, nameString);
      return;
    }

    // Generic component deserialization
    const ComponentClass = ComponentRegistry.get(packet.componentId);
    if (!ComponentClass) {
      console.warn(`No component registered for type: ${packet.componentId}`);
      return;
    }

    const reader = new EvPacketReader(packet.data);
    const component = (ComponentClass as any).fromBuffer(packet.ntt, reader);

    // Handle side effects before setting component
    this.handleSideEffects(packet.componentId, component, entity);

    entity.set(component);
  }

  private static handleSideEffects(componentId: ComponentTypeId, component: any, entity: any): void {
    switch (componentId) {
      case ServerComponentType.Physics: {
        // Create RenderComponent if missing
        if (!entity.has(Components.RenderComponent)) {
          entity.set(new Components.RenderComponent(entity.id, {
            sides: component.sides,
            shapeType: component.sides === 3 ? 1 : component.sides === 4 ? 2 : 0,
            color: component.color,
            shipParts: [],
          }));
        }

        // Update NetworkComponent with server position
        let network = entity.get(Components.NetworkComponent);
        if (!network) {
          network = new Components.NetworkComponent(entity.id, {
            serverId: entity.id,
            isLocallyControlled: entity.id === (window as any).localPlayerId,
            serverPosition: component.lastPosition,
            serverVelocity: { x: 0, y: 0 },
            serverRotation: component.lastRotation,
          });
          entity.set(network);
        } else {
          network.serverPosition = component.lastPosition;
          network.serverRotation = component.lastRotation;
        }
        break;
      }

      case ServerComponentType.ParentChild: {
        // Dispatch event for ship building UI
        window.dispatchEvent(new CustomEvent("ship-part-confirmed", {
          detail: {
            ntt: entity.id,
            parentId: component.parentId,
            gridX: component.gridX,
            gridY: component.gridY,
          },
        }));
        break;
      }
    }
  }

  static fromBuffer(buffer: ArrayBuffer): ComponentStatePacket {
    const reader = new EvPacketReader(buffer);
    const header = reader.Header();
    const ntt = reader.Guid();
    const componentId = reader.i8();
    const dataLength = reader.i16();
    const data = buffer.slice(reader.currentOffset, reader.currentOffset + dataLength);

    return new ComponentStatePacket(header, NTT.from(ntt), componentId, dataLength, data);
  }
}
```

### EvPacketReader

**File**: `pixiejsClient/src/app/network/EvPacketReader.ts`

```typescript
export class EvPacketReader {
  private view: DataView;
  private offset = 0;
  private buffer: ArrayBuffer;
  private uint8View: Uint8Array;

  constructor(buffer: ArrayBuffer) {
    this.buffer = buffer;
    this.view = new DataView(buffer);
    this.uint8View = new Uint8Array(buffer);
  }

  get currentOffset(): number { return this.offset; }

  Header(): PacketHeader {
    const length = this.i16();
    const id = this.i16();
    return new PacketHeader(length, id);
  }

  Guid(): string {
    const guidBytes = this.uint8View.slice(this.offset, this.offset + 16);
    this.offset += 16;

    // Reconstruct GUID string with proper byte ordering
    return (
      `${hex[guidBytes[3]]}${hex[guidBytes[2]]}${hex[guidBytes[1]]}${hex[guidBytes[0]]}-` +
      `${hex[guidBytes[5]]}${hex[guidBytes[4]]}-` +
      `${hex[guidBytes[7]]}${hex[guidBytes[6]]}-` +
      `${hex[guidBytes[8]]}${hex[guidBytes[9]]}-` +
      `${hex[guidBytes[10]]}${hex[guidBytes[11]]}${hex[guidBytes[12]]}` +
      `${hex[guidBytes[13]]}${hex[guidBytes[14]]}${hex[guidBytes[15]]}`
    );
  }

  i64(): bigint {
    const value = this.view.getBigUint64(this.offset, true);
    this.offset += 8;
    return value;
  }

  i32(): number {
    const value = this.view.getInt32(this.offset, true);
    this.offset += 4;
    return value;
  }

  i16(): number {
    const value = this.view.getInt16(this.offset, true);
    this.offset += 2;
    return value;
  }

  i8(): number {
    const value = this.view.getInt8(this.offset);
    this.offset += 1;
    return value;
  }

  f32(): number {
    const value = this.view.getFloat32(this.offset, true);
    this.offset += 4;
    return value;
  }

  f64(): number {
    const value = this.view.getFloat64(this.offset, true);
    this.offset += 8;
    return value;
  }
}
```

### Component Base Class (Client)

**File**: `pixiejsClient/src/app/ecs/core/Component.ts`

```typescript
// Decorator for auto-registering components
export function component(componentType: ComponentTypeId) {
  return function (target: any) {
    ComponentRegistry.set(componentType, target);
    target.prototype.componentType = componentType;
  };
}

// Decorator for defining serialization order and type
export function serverField(index: number, type: FieldType, options?: { skip?: boolean }) {
  return function (target: any, propertyKey: string) {
    const constructor = target.constructor;
    if (!fieldMetadata.has(constructor)) {
      fieldMetadata.set(constructor, []);
    }
    fieldMetadata.get(constructor)!.push({
      propertyKey,
      index,
      type,
      skip: options?.skip,
    });
  };
}

export abstract class Component {
  readonly ntt: NTT;
  componentType!: ComponentTypeId;
  @serverField(0, "i64") public changedTick: bigint = 0n;

  fromBuffer(reader: EvPacketReader): void {
    // Collect fields from inheritance chain
    const allFields: FieldMetadata[] = [];
    let currentClass = this.constructor;
    while (currentClass) {
      const metadata = fieldMetadata.get(currentClass);
      if (metadata) allFields.push(...metadata);
      currentClass = Object.getPrototypeOf(currentClass);
      if (currentClass === Function.prototype) break;
    }

    // Sort by index and deserialize
    const sorted = allFields.sort((a, b) => a.index - b.index);
    for (const field of sorted) {
      const value = this.readField(reader, field.type);
      if (!field.skip) {
        (this as any)[field.propertyKey] = value;
      }
    }
  }

  protected readField(reader: EvPacketReader, type: FieldType): any {
    switch (type) {
      case "i8":     return reader.i8();
      case "u8":     return reader.i8() & 0xff;
      case "i16":    return reader.i16();
      case "u16":    return reader.u16();
      case "i32":    return reader.i32();
      case "u32":    return reader.u32();
      case "i64":    return reader.i64();
      case "f32":    return reader.f32();
      case "f64":    return reader.f64();
      case "bool":   return reader.i8() !== 0;
      case "guid":   return reader.Guid();
      case "vector2": return { x: reader.f32(), y: reader.f32() };
      case "string64": {
        const bytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) bytes[i] = reader.i8();
        const nullIndex = bytes.indexOf(0);
        return new TextDecoder().decode(bytes.subarray(0, nullIndex >= 0 ? nullIndex : 64));
      }
    }
  }

  static fromBuffer<T extends Component>(this: new (ntt: NTT) => T, ntt: NTT, reader: EvPacketReader): T {
    const instance = new this(ntt);
    instance.fromBuffer(reader);
    return instance;
  }
}
```

**FieldType Mapping**:

| FieldType | C# Type | JS Read Method | Size |
|-----------|---------|----------------|------|
| `i8` | `sbyte` | `getInt8()` | 1 |
| `u8` | `byte` | `getInt8() & 0xff` | 1 |
| `i16` | `short` | `getInt16(_, true)` | 2 |
| `u16` | `ushort` | `getUint16(_, true)` | 2 |
| `i32` | `int` | `getInt32(_, true)` | 4 |
| `u32` | `uint` | `getUint32(_, true)` | 4 |
| `i64` | `long` | `getBigUint64(_, true)` | 8 |
| `f32` | `float` | `getFloat32(_, true)` | 4 |
| `f64` | `double` | `getFloat64(_, true)` | 8 |
| `bool` | `bool` | `getInt8() !== 0` | 1 |
| `guid` | `Guid` | Custom GUID parse | 16 |
| `vector2` | `Vector2` | 2× `f32` | 8 |
| `string64` | `[MarshalAs(..., SizeConst=64)]` | 64 bytes → TextDecoder | 64 |

---

## Component Mapping Reference

### Example: HealthComponent

**Server** (`server/Simulation/Components/HealthComponent.cs`):

```csharp
[Component(ComponentType = ComponentType.Health, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HealthComponent(float health, float maxHealth)
{
    /// MUST be first field for raw byte access in ComponentSerializer
    public long ChangedTick = NttWorld.Tick;

    public float Health = health;
    public float MaxHealth = maxHealth;
}
```

**Client** (`pixiejsClient/src/app/ecs/components/HealthComponent.ts`):

```typescript
@component(ServerComponentType.Health)
export class HealthComponent extends Component {
  // changedTick inherited from Component base (index 0)
  @serverField(1, "f32") Health: number;
  @serverField(2, "f32") MaxHealth: number;

  constructor(ntt: NTT, health?: number, maxHealth?: number) {
    super(ntt);
    this.Health = health ?? 100;
    this.MaxHealth = maxHealth ?? 100;
  }
}
```

**Binary Layout** (16 bytes total):

```
Offset  Size  Type   Field
──────  ────  ────   ─────
0       8     i64    ChangedTick
8       4     f32    Health
12      4     f32    MaxHealth
```

### Example: PhysicsComponent (Complex)

**Client** (`pixiejsClient/src/app/ecs/components/PhysicsComponent.ts`):

```typescript
@component(ServerComponentType.Physics)
export class PhysicsComponent extends Component {
  // Index 0: changedTick (inherited)
  @serverField(1, "i64", { skip: true }) bodyId: bigint = 0n;  // Skip: B2BodyId not used in JS
  @serverField(2, "bool") isStatic: boolean = false;
  @serverField(3, "u32") color: number;
  @serverField(4, "f32") density: number;
  @serverField(5, "i32") sides: number;
  @serverField(6, "vector2") lastPosition: Vector2;
  @serverField(7, "f32") lastRotation: number;

  // Client-only properties (not serialized)
  position: Vector2;
  rotationRadians: number;
  linearVelocity: Vector2;
  // ... etc
}
```

**Notes**:
- `skip: true` reads the field (to advance offset) but doesn't assign it
- Client-only fields are initialized in constructor, not from network

---

## Dirty Tracking Mechanism

### ChangedTick Contract

**Rule**: Every network-synced component MUST have `ChangedTick` as its **first field**.

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SomeComponent
{
    public long ChangedTick;  // MUST BE FIRST
    // ... other fields
}
```

**Why First?**: `ComponentSerializer.TrySyncTyped<T>` reads `ChangedTick` via raw byte access:

```csharp
var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref component, 1));
var changedTick = MemoryMarshal.Read<long>(bytes);  // Reads first 8 bytes
```

### Setting ChangedTick

Systems that modify components must set `ChangedTick = NttWorld.Tick`:

```csharp
// In DamageSystem
ref var health = ref entity.Get<HealthComponent>();
health.Health -= damage.Amount;
health.ChangedTick = NttWorld.Tick;  // Mark dirty for this tick
```

### Sync Decision Logic

```csharp
if (changedTick == NttWorld.Tick)
    viewer.NetSync(Serialize(entity, ref component));
```

**Behavior**:
- `==` exact match: Component syncs only if modified **this exact tick**
- If modified tick N-1, it won't sync on tick N
- On component creation, set `ChangedTick = NttWorld.Tick` to ensure initial sync

---

## Viewport Culling Integration

### ViewportSystem

**File**: `server/Simulation/Systems/ViewportSystem.cs`

```csharp
public sealed class ViewportSystem : NttSystem<PhysicsComponent, ViewportComponent>
{
    public override void Update(in NTT ntt, ref PhysicsComponent body, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>()) return;

        // Only update periodically (every 10 ticks) for performance
        bool firstUpdate = vwp.EntitiesVisible.Count == 0 && vwp.EntitiesVisibleLast.Count == 0;
        bool periodicUpdate = NttWorld.Tick % 10 == 0;
        if (!firstUpdate && !periodicUpdate) return;

        // Center viewport on player
        vwp.Viewport.X = body.Position.X - vwp.Viewport.Width / 2;
        vwp.Viewport.Y = body.Position.Y - vwp.Viewport.Height / 2;

        // Track previous visible set for delta detection
        vwp.EntitiesVisibleLast.Clear();
        vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
        vwp.EntitiesVisible.Clear();

        // AABB intersection test against all physics entities
        foreach (var entity in NttQuery.Query<PhysicsComponent>())
        {
            var pos = entity.Get<PhysicsComponent>().Position;

            // Simple AABB test
            if (pos.X + 0.5f >= vwp.Viewport.X &&
                pos.X - 0.5f <= vwp.Viewport.X + vwp.Viewport.Width &&
                pos.Y + 0.5f >= vwp.Viewport.Y &&
                pos.Y - 0.5f <= vwp.Viewport.Y + vwp.Viewport.Height)
            {
                vwp.EntitiesVisible.Add(entity);
            }
        }
    }
}
```

**Integration with ComponentSyncSystem**:

```csharp
foreach (var visibleEntity in vwp.EntitiesVisible)
{
    SyncEntity(ntt, visibleEntity);
    SyncChildEntities(ntt, visibleEntity);
}
```

**Bandwidth Impact**:
- Without culling: All entities synced to all players
- With culling: Only entities in viewport (~screen size + margin)
- Typical reduction: 10-100x fewer entities synced per player

---

## Packet Batching & Network Layer

### PacketQueue

**File**: `server/Helpers/PacketQueue.cs`

```csharp
public static class PacketQueue
{
    private static readonly ConcurrentDictionary<NTT, Queue<Memory<byte>>> Packets = new();

    public static void Enqueue(in NTT player, in Memory<byte> packet)
    {
        if (!Packets.TryGetValue(player, out var queue))
        {
            queue = new Queue<Memory<byte>>();
            Packets.TryAdd(player, queue);
        }
        queue.Enqueue(packet);
    }

    public static void FlushAll()
    {
        foreach (var (ntt, queue) in Packets)
        {
            if (queue.Count == 0) continue;

            var net = ntt.Get<NetworkComponent>();

            // Fast path: single packet
            if (queue.Count == 1)
            {
                var packet = queue.Dequeue();
                _ = net.Socket.SendAsync(packet, WebSocketMessageType.Binary, true, CancellationToken.None);
                continue;
            }

            // Batch multiple packets into single buffer
            var totalSize = 0;
            foreach (var packet in queue)
                totalSize += packet.Length;

            var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
            try
            {
                var offset = 0;
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    packet.Span.CopyTo(buffer.AsSpan(offset));
                    offset += packet.Length;
                }

                // Single WebSocket send for all packets
                _ = net.Socket.SendAsync(
                    new Memory<byte>(buffer, 0, totalSize),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None
                );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
```

**Benefits of Batching**:
- Reduces WebSocket frame overhead (header per message)
- Reduces TCP segment overhead
- Single syscall for multiple packets
- Uses `ArrayPool` for zero-allocation batching

### NTT.NetSync

```csharp
internal void NetSync(Memory<byte> buffer) => PacketQueue.Enqueue(in this, in buffer);
```

Packets are queued during `ComponentSyncSystem.Update()`, then flushed after all systems complete via `PacketQueue.FlushAll()`.

---

## Implementation Examples

### Adding a New Synced Component

**Step 1: Define Server Component**

```csharp
// server/Simulation/Components/ExampleComponent.cs
[Component(ComponentType = ComponentType.Example, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ExampleComponent
{
    public long ChangedTick;     // MUST be first
    public float Value;
    public int State;
    public bool Active;
}
```

**Step 2: Add to ComponentType Enum**

```csharp
// server/Enums/ComponentIds.cs
public enum ComponentType : byte
{
    // ... existing
    Example = 50,
}
```

**Step 3: Define Client Component**

```typescript
// pixiejsClient/src/app/ecs/components/ExampleComponent.ts
import { Component, component, serverField } from "../core/Component";
import { ServerComponentType } from "../../enums/ComponentIds";
import { NTT } from "../core/NTT";

@component(ServerComponentType.Example)
export class ExampleComponent extends Component {
  @serverField(1, "f32") value: number = 0;
  @serverField(2, "i32") state: number = 0;
  @serverField(3, "bool") active: boolean = false;

  constructor(ntt: NTT) {
    super(ntt);
  }
}
```

**Step 4: Add to Client ComponentIds**

```typescript
// pixiejsClient/src/app/enums/ComponentIds.ts
export const ServerComponentType = {
  // ... existing
  Example: 50,
} as const;
```

**Step 5: Export Component**

```typescript
// pixiejsClient/src/app/ecs/components/index.ts
export * from "./ExampleComponent";
```

The system automatically handles registration via the `@component` decorator.

### Adding Side Effects

If the component needs special handling on the client:

```typescript
// In ComponentStatePacket.handleSideEffects()
case ServerComponentType.Example: {
  const example = component as Components.ExampleComponent;
  if (example.active) {
    window.dispatchEvent(new CustomEvent("example-activated", {
      detail: { entityId: entity.id, value: example.value }
    }));
  }
  break;
}
```

---

## Performance Characteristics

### Packet Size Analysis

**Per-Component Overhead**: 23 bytes fixed
- Header: 4 bytes
- Entity ID: 16 bytes
- Component Type: 1 byte
- Data Length: 2 bytes

**Example Packet Sizes**:
| Component | Struct Size | Total Packet |
|-----------|-------------|--------------|
| HealthComponent | 16 bytes | 39 bytes |
| PhysicsComponent | ~40 bytes | 63 bytes |
| ParentChildComponent | 24 bytes | 47 bytes |

### Bandwidth Estimation

**Assumptions**:
- 60 TPS (ticks per second)
- 50 visible entities per player
- Average 3 components changed per entity per tick
- Average component size: 40 bytes

**Calculation**:
```
Per tick:  50 entities × 3 components × (23 + 40) bytes = 9,450 bytes
Per second: 9,450 × 60 = 567 KB/s = 4.5 Mbps
```

**With batching**: Single WebSocket frame per tick reduces overhead significantly.

### CPU Considerations

**Server**:
- Reflection at startup only (cached)
- `MemoryMarshal.AsBytes()` is zero-copy
- `ArrayPool` eliminates allocation in hot path

**Client**:
- Decorator metadata cached at class load
- `DataView` operations are native JS
- Single pass through packet buffer

---

## Summary

The ComponentSync system provides efficient, type-safe state synchronization through:

1. **Declarative Metadata**: `[Component]` attribute + `@component`/`@serverField` decorators
2. **Binary Protocol**: Compact struct serialization with deterministic layout
3. **Dirty Tracking**: `ChangedTick` comparison for minimal network traffic
4. **Viewport Culling**: Only sync visible entities
5. **Packet Batching**: Combine multiple packets per WebSocket frame
6. **Side Effect Hooks**: Component-specific logic on client receipt

**Critical Constraints**:
- `ChangedTick` MUST be first field in all synced components
- `[StructLayout(LayoutKind.Sequential, Pack = 1)]` required for deterministic layout
- Field indices in `@serverField` must match C# struct field order
- Little-endian byte order throughout

---

**End of Specification**
