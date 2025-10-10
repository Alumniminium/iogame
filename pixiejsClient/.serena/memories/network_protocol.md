# Network Protocol

## Communication Layer
- **Protocol**: WebSockets (binary)
- **Endpoint**: `/ws` on game server
- **Architecture**: Fully server-authoritative

## Packet System
- Packets defined in `src/app/network/packets/`
- Packet IDs in `src/app/enums/PacketIds.ts`
- Binary serialization for efficiency

### Key Packet Types
- **ComponentStatePacket** - Component data sync from server
- **EntitySyncPacket** - Entity creation/deletion
- **InputPacket** - Player input to server
- **Various game-specific packets** - Chat, spawn, death, etc.

## Network Flow

### Client → Server
1. Player input captured by InputSystem
2. Packed into InputPacket
3. Sent to server via WebSocket

### Server → Client
1. Server runs physics simulation (60 TPS)
2. ComponentSyncSystem identifies changed components
3. Only changed data sent to clients in viewport
4. Client NetworkSystem applies updates directly

## Synchronization Strategy
- **No client-side prediction** - Client trusts server completely
- **Direct position updates** - NetworkSystem sets exact server positions
- **Visual interpolation** - RenderSystem lerps graphics for smoothness
- **Viewport culling** - Only entities in view are synchronized

## Performance Optimizations
- Binary packet format (not JSON)
- Component-based sync (not full entity state)
- Viewport culling reduces bandwidth
- Changed-only updates minimize data transfer
