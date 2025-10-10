# Networking Architecture

## Communication Protocol
- **WebSocket-based** real-time communication
- **Binary packet protocol** for efficiency
- **Server-authoritative** - all game logic on server
- **Component-based sync** - only changed components sent

## Network Flow
1. Client connects via WebSocket (`/ws` endpoint)
2. Client sends input packets to server
3. Server processes input in InputSystem
4. Server runs physics and game logic (60 TPS)
5. ComponentSyncSystem detects changed components
6. Server sends only changed components to clients in viewport
7. Client applies server state directly (no prediction)

## Viewport Culling
- ViewportSystem determines visible entities per client
- Clients only receive data for entities in their viewport
- Reduces bandwidth and improves performance
- Large maps with many entities remain performant

## Component Synchronization
- Components marked with `NetworkSync = true` are synced
- ChangedTick field tracks when component was modified
- ComponentSyncSystem compares ChangedTick to last sync
- Only components with ChangedTick > LastSyncTick are sent
- Efficient delta-based synchronization

## Packet Handling
- **Server-side**: `Simulation/Net/PacketHandler.cs`
- **Client-side**: TypeScript packet classes in pixiejsClient
- Binary serialization for compact wire format
- Packet ID enum for type identification

## Serialization
- `Serialization/ComponentSerializer.cs` handles component serialization
- Raw byte access to struct data for performance
- ChangedTick MUST be first field for correct byte offset
- `StructLayout(LayoutKind.Sequential, Pack = 1)` required

## Input Handling
- Client sends input state to server
- Server processes in InputSystem
- Input latency: client -> server -> physics -> client
- No client-side prediction (server-authoritative design)

## Performance Considerations
- 60 TPS (16.67ms per tick)
- Only changed components synced
- Viewport culling reduces network traffic
- Binary protocol minimizes bandwidth
- Component-based sync scales well