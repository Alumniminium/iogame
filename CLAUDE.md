# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

**Build the server:**
```bash
dotnet build server/server.csproj
```

**Run the server in development mode:**
```bash
cd server && dotnet run
```

**Run the server with debugger (VSCode):**
Use the "SERVER" launch configuration in `.vscode/launch.json`

**Build the entire solution:**
```bash
dotnet build iogame.sln
```

**PixiJS client development:**
The pixiejsClient uses Vite for development:
```bash
cd pixiejsClient && npm run dev
```

**Build the PixiJS client:**
```bash
cd pixiejsClient && npm run build
```

**Lint the PixiJS client:**
```bash
cd pixiejsClient && npm run lint
```

## Architecture Overview

This is a multiplayer IO game with a custom **Entity Component System (ECS)** architecture:

### Project Structure
- **server/** - .NET 9 ASP.NET Core server with custom ECS game engine
  - **NttECS/** - Core ECS framework (NttWorld, NttSystem, PackedComponentStorage)
  - **Simulation/** - Game-specific components, systems, and managers
  - **Helpers/** - Utility classes (IncomingPacketQueue, PerformanceMetrics, Vector2Ext)
  - **Serialization/** - Component serialization for network sync
  - **Enums/** - Shared enumerations (ComponentType, ShapeType, PlayerInput)
- **pixiejsClient/** - Modern TypeScript PixiJS client with client-side ECS
  - **src/app/ecs/** - Client-side ECS (Entity, Component, System, World)
  - **src/app/network/** - Network manager and packet handlers
  - **src/app/ui/** - Game UI components (HUD, stats panels, chat, pause menu)
  - **src/app/managers/** - Game managers (Input, PlayerName, ShipPart)

### Core ECS Architecture (server/NttECS/)
- **NTT** - Lightweight entity structs with parent-child relationships and component storage
- **NttWorld** - Central ECS coordinator managing entities, systems, and tick-based simulation
- **NttSystem** - Base class for systems with automatic entity filtering and multi-threading support
- **PackedComponentStorage** - High-performance component storage using struct-of-arrays layout
- **Components** - Data-only structs in server/Simulation/Components/
- **Systems** - Logic processors in server/Simulation/Systems/

### Game Loop Order (server/Simulation/Game.cs)
Systems execute in this exact order each tick:
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

### Client Architecture (pixiejsClient/)
- **PixiJS-based** 2D rendering with WebGL acceleration
- **Client-side ECS** mirrors server component structure
- **Systems**:
  - **InputSystem** - Capture and process player input
  - **NetworkSystem** - Handle incoming packets and entity sync (directly applies server positions)
  - **RenderSystem** - Render entities, shields, particles with camera transforms (visually lerps graphics for smooth rendering)
  - **ParticleSystem** - Update particle effects
  - **LifetimeSystem** - Remove expired entities
  - **BuildModeSystem** - Ship building interface logic
  - **ShipPartSyncSystem** - Sync ship part data for rendering
- **No client prediction** - Client directly applies server-authoritative positions
- **Visual interpolation** - Graphics lerp toward physics positions for smooth rendering (60 FPS)

### Networking Architecture
- **WebSocket-based** real-time communication via `/ws` endpoint
- **Binary packet protocol** defined in pixiejsClient/src/app/network/packets/
- **Component-based sync** - Only changed components are sent to clients
- **Fully server-authoritative** - Server runs physics at 60 TPS, clients render server state directly
- **Viewport culling** - Clients only receive entity data within their viewport
- **Input latency** - Client sends input to server, waits for server physics response

### Key Performance Patterns
- **Struct-of-arrays** packed component storage for SIMD and cache efficiency
- **Lock-free thread-safe pooling** using Interlocked operations (Pool<T>)
- **SIMD vectorization** for high-throughput operations (SwapList<T>)
- **Multi-threaded systems** - Systems can process entities across multiple threads
- **Parent-child transforms** for complex multi-part ships
- **Zero-allocation patterns** throughout hot paths

## Common Development Patterns

**Adding new components:**
1. Create data-only struct in server/Simulation/Components/
2. Add [Component(ComponentType = ComponentType.YourComponent, NetworkSync = true/false)] attribute
3. Add component type to server/Enums/ComponentIds.cs enum
4. Add matching TypeScript class in pixiejsClient/src/app/ecs/components/
5. Add to ComponentType enum in pixiejsClient/src/app/enums/ComponentIds.ts
6. Add deserialization case in pixiejsClient/src/app/network/packets/ComponentStatePacket.ts

**Adding new systems:**
1. Inherit from NttSystem<T1, T2, ...> in server/Simulation/Systems/
2. Override `Update(in NTT ntt, ref T1 c1, ref T2 c2, ...)` method
3. Register system in Game.cs systems list (order matters!)
4. Systems are NOT auto-registered - must be manually added to Game.cs

**Adding new packet types:**
1. Define packet class in pixiejsClient/src/app/network/packets/
2. Add to PacketId enum in pixiejsClient/src/app/network/PacketIds.ts
3. Implement handler in server/Simulation/Net/PacketHandler.cs
4. Add client-side handling in NetworkManager or appropriate system

**Entity spawning:**
Use SpawnManager.cs for consistent entity creation with proper component initialization.

## Physics and Coordinate Systems

**CRITICAL: Box2D Coordinate System (NEVER GET THIS WRONG AGAIN!)**
- **Positive Y = DOWN, Negative Y = UP** (gravity is +9.81 Y)
- **Positive X = RIGHT, Negative X = LEFT**
- **0° rotation = pointing RIGHT (positive X)**
- **+90° rotation = pointing DOWN (positive Y)**
- **-90° rotation = pointing UP (negative Y)**
- **+180° rotation = pointing LEFT (negative X)**

**Force Application Rules:**
- Apply forces in the direction you want the object to move
- For upward thrust: use negative Y force to counteract positive Y gravity
- Standard forward direction: `new Vector2(MathF.Cos(rotation), MathF.Sin(rotation))`
- To point UP: spawn with -90° rotation, which gives forward = (0, -1) = UP

**Mass vs Density in Box2D:**
- Box2D uses density, not mass directly
- **Actual mass = density × area**
- For desired mass: `density = desiredMass / (width × height)`

## Code Conventions

- Use file-scoped namespaces: `namespace MyNamespace;`
- No braces for single-line if statements
- Unsafe code enabled for performance-critical paths
- .NET 9 (net9) with preview language features
- Nullable reference types disabled for compatibility
- Don't run the frontend or server. The processes do not exit and only the user can test. Ask them to run and test when needed.

## Important Performance Notes

- **SwapList<T>** uses SIMD vectorization for Contains() and IndexOf() - 8x-16x faster on vectorizable types
- **Pool<T>** is lock-free and zero-allocation during runtime - all storage pre-allocated
- **MultiThreadWorkQueue** processes work items across dedicated worker threads
- Systems with `threads > 1` automatically partition entity processing across threads
