# IOGame Codebase Comprehensive Index

**Last Updated**: 2025-10-18
**Target Framework**: .NET 9 (Server), TypeScript/PixiJS (Client)
**Architecture**: Custom Entity Component System (ECS)
**Networking**: WebSocket-based binary protocol

---

## Project Overview

IOGame is a multiplayer browser-based IO game featuring a .NET 9 ASP.NET Core server with a custom ECS framework and a PixiJS TypeScript client. The architecture emphasizes performance through:
- Struct-of-arrays packed component storage
- Lock-free thread-safe pooling
- SIMD vectorization
- Multi-threaded system processing
- Server-authoritative physics at 60 TPS
- Viewport-based entity culling

---

## Directory Structure

```
/iogame/
├── server/                          (.NET 9 ASP.NET Core server)
│   ├── NttECS/                      (Custom ECS Framework)
│   │   ├── ECS/                     (Core ECS classes)
│   │   ├── Memory/                  (Performance structures)
│   │   ├── Threading/               (Multi-threading utilities)
│   │   └── Utilities/               (Helper utilities)
│   ├── Simulation/                  (Game logic)
│   │   ├── Components/              (28 component definitions)
│   │   ├── Systems/                 (21 game systems)
│   │   ├── Managers/                (Entity managers)
│   │   ├── Database/                (Resource definitions)
│   │   └── Net/                     (Networking & packets)
│   ├── Enums/                       (Shared enumerations)
│   ├── Helpers/                     (Utility classes)
│   ├── Serialization/               (Component serialization)
│   ├── Program.cs                   (Server entry point)
│   └── Startup.cs                   (ASP.NET configuration)
│
├── pixiejsClient/                   (PixiJS TypeScript Client)
│   ├── src/
│   │   ├── app/
│   │   │   ├── ecs/                 (Client-side ECS)
│   │   │   │   ├── core/            (ECS base classes)
│   │   │   │   ├── components/      (30 component definitions)
│   │   │   │   ├── systems/         (4 core systems + renderers)
│   │   │   │   └── effects/         (Particle effects)
│   │   │   ├── network/             (Network communication)
│   │   │   │   ├── packets/         (6 packet types)
│   │   │   │   └── managers/        (Network managers)
│   │   │   ├── managers/            (12 game managers)
│   │   │   ├── enums/               (Component IDs, Effect types)
│   │   │   ├── ui/                  (UI components)
│   │   │   │   ├── game/            (In-game UI)
│   │   │   │   └── shipbuilder/     (Ship building UI)
│   │   │   ├── screens/             (Game screens)
│   │   │   └── utils/               (Utilities)
│   │   ├── engine/                  (Pixi engine wrapper)
│   │   │   ├── audio/               (Audio system)
│   │   │   ├── navigation/          (Screen navigation)
│   │   │   ├── resize/              (Resize handling)
│   │   │   └── utils/               (Engine utilities)
│   │   └── main.ts                  (Client entry point)
│   ├── package.json                 (Dependencies)
│   ├── tsconfig.json                (TypeScript config)
│   ├── vite.config.ts               (Vite build config)
│   └── scripts/                     (Build scripts)
│
├── .vscode/                         (VSCode configuration)
│   ├── launch.json                  (Debug configurations)
│   ├── tasks.json                   (Build tasks)
│   └── settings.json                (Editor settings)
│
└── iogame.sln                       (Visual Studio solution)
```

---

## Server Architecture (C# - .NET 9)

### Core Framework (`server/NttECS/`)

#### ECS Core (`server/NttECS/ECS/`)
- **NTT.cs** (81 lines)
  - Core entity struct with GUID identification
  - Generic component methods: `Set<T>()`, `Get<T>()`, `Has<T>()`, `Remove<T>()`
  - Multi-component query support: `Has<T1, T2, T3, ...>()`
  
- **NttWorld.cs** (~300 lines)
  - Central world coordinator for ECS
  - Entity lifecycle management (creation, destruction)
  - System registration and tick-based update loop
  - Parent-child entity relationships
  - Player tracking
  - Persistent state serialization to JSON
  - Target: 60 TPS

- **NttSystem.cs** (~150 lines)
  - Abstract base class for all systems
  - Entity filtering by component requirements
  - Multi-threaded processing support
  - `BeginUpdate()` / `EndUpdate()` for threaded work distribution
  - `Update(start, count)` abstract method for entity processing
  - Delta time and tick information

- **NttQuery.cs**
  - Entity query system for component filtering
  
- **ComponentAttribute.cs**
  - Custom attribute marking components for auto-registration
  - Properties: ComponentType, NetworkSync flag

- **PackedComponentStorage.cs**
  - High-performance struct-of-arrays component storage
  - Lock-free design for thread safety
  - Supports SIMD vectorization
  - Automatic entity filtering based on component presence

- **ReflectionHelper.cs**
  - Reflection utilities for component management
  - Auto-registration of component types
  - Recycling components back to pools

- **ThreadedWorker.cs**
  - Multi-threaded entity processing
  - Work distribution across N threads
  - Synchronization and work partitioning

#### Memory Performance (`server/NttECS/Memory/`)
- **Pool<T>.cs**
  - Lock-free object pooling using Interlocked operations
  - Zero-allocation pattern for hot paths
  - Pre-allocated storage for runtime efficiency

- **SwapList<T>.cs**
  - SIMD-optimized collection
  - 8x-16x faster Contains() and IndexOf() on vectorizable types
  - Used for rapid entity iteration

#### Threading (`server/NttECS/Threading/`)
- **MultiThreadWorkQueue.cs**
  - Dedicated worker threads for parallel entity processing
  - Work item distribution
  - Thread pool management

---

### Simulation Systems (`server/Simulation/`)

#### Components (`server/Simulation/Components/`) - 28 Components
**Physics & Collision:**
- `Box2DBodyComponent.cs` - Box2D physics body wrapper
- `CollisionComponent.cs` - Collision state tracking
- `GravityComponent.cs` - Gravity source definition

**Health & Damage:**
- `HealthComponent.cs` - Current/max health
- `HealthRegenComponent.cs` - Health regeneration rate
- `BodyDamageComponent.cs` - Body part damage tracking
- `DamageComponent.cs` - Damage value and source
- `ShieldComponent.cs` - Shield absorption and recharge

**Energy & Power:**
- `EnergyComponent.cs` - Power generation, consumption, storage
- `EngineComponent.cs` - Thrust power definition

**Entity Data:**
- `NetworkComponent.cs` - WebSocket and network state
- `NameTagComponent.cs` - Player name (fixed 64-byte array)
- `ViewportComponent.cs` - View culling bounds
- `ParentChildComponent.cs` - Multi-part ship relationships

**Gameplay:**
- `InputComponent.cs` - Current player input state
- `WeaponComponent.cs` - Weapon fire state and cooldown
- `LevelComponent.cs` - Player level and experience
- `InventoryComponent.cs` - Resource inventory
- `BulletComponent.cs` - Projectile definition
- `PickableComponent.cs` - Pickable resource definition
- `PickupComponent.cs` - Pickup item definition
- `SpawnerComponent.cs` - Entity spawner configuration

**State Tags:**
- `DeathTagComponent.cs` - Entity marked for death
- `RespawnTagComponent.cs` - Player respawn marker

**Rendering & Display:**
- `ColorComponent.cs` - RGB color (0xRRGGBB)
- `LifeTimeComponent.cs` - Entity expiration timer
- `ExpRewardComponent.cs` - Experience granted on death
- `EffectComponent.cs` - Visual effects

#### Systems (`server/Simulation/Systems/`) - 21 Systems
**Game Loop Order** (as defined in `Game.cs`):

1. **SpawnSystem** - Entity creation, spawner processing
2. **ViewportSystem** - Viewport culling for network optimization
3. **InputSystem** - Player input packet processing
4. **PositionSyncSystem** - Sync physics position changes for client prediction
5. **ShipPhysicsRebuildSystem** - Rebuild Box2D bodies when ship changes
6. **GravitySystem** - Apply gravity forces from gravity sources
7. **EngineSystem** - Process engine thrust and RCS using Box2D
8. **EnergySystem** - Energy generation, consumption, battery management
9. **ShieldSystem** - Shield charge/recharge and power consumption
10. **WeaponSystem** - Weapon firing and projectile spawning
11. **PickupCollisionResolver** - Handle pickup collection
12. **ProjectileCollisionSystem** - Handle projectile collisions
13. **DamageSystem** - Apply damage to entities
14. **HealthSystem** - Health regeneration and death processing
15. **DropSystem** - Handle entity drops on death
16. **LifetimeSystem** - Remove expired entities
17. **LevelExpSystem** - Experience and leveling
18. **RespawnSystem** - Player respawn logic
19. **ComponentSyncSystem** - Generic component sync to clients
20. **DeathSystem** - Final cleanup for dead entities

Additional non-ordered systems:
- **CollisionSystem** - Physics collision handling

#### Managers (`server/Simulation/Managers/`)
- **SpawnManager.cs** - Consistent entity creation with proper initialization

#### Utilities

**Enums** (`server/Enums/`):
- **ComponentIds.cs** - ComponentType enum (43 component types)
- **PacketId.cs** - PacketId enum (10+ packet types)
- **PlayerInput.cs** - Input button definitions
- **ShapeType.cs** - Physics shape types (Circle, Box, Polygon)
- **CollisionCategory.cs** - Physics collision categories
- **EffectType.cs** - Visual effect types

**Helpers** (`server/Helpers/`):
- **Vector2Ext.cs** - Vector2 extension methods
- **PerformanceMetrics.cs** - Timing and profiling
- **NumberFormater.cs** - Number formatting utilities
- **FastNoise.cs** - Perlin/Simplex noise generation
- **IncomingPacketQueue.cs** - Thread-safe incoming packet queue
- **OutgoingPacketQueue.cs** - Thread-safe outgoing packet queue
- **PacketQueue.cs** - Base packet queue implementation

**Serialization** (`server/Serialization/`):
- **ComponentSerializer.cs** - Serialize/deserialize components to binary

#### Networking (`server/Simulation/Net/`)
- **PacketHandler.cs** - Central packet routing and processing
- **PacketReader.cs** - Binary packet deserialization
- **PacketWriter.cs** - Binary packet serialization
- **Header.cs** - Packet header format
- **LoginRequestPacket.cs** - Player login
- **LoginResponsePacket.cs** - Server login response
- **ChatPacket.cs** - Chat messages
- **PingPacket.cs** - Keepalive pings
- **RequestSpawnPacket.cs** - Player spawn requests

#### Database (`server/Simulation/Database/`)
- **Database.cs** - Base resource definitions (colored polygons 3-8 sides)
- **BaseResource.cs** - Resource properties (health, color, elasticity, drag, spawn limits)

#### Game Coordinator (`server/Simulation/`)
- **Game.cs** - Main game initialization, system setup, game loop
  - Map size: 32,000 x 32,000 units
  - Gravity sources at top and bottom edges
  - Asteroid field initialization
  - Fixed 60 TPS simulation

- **PhysicsWorld.cs** - Box2D physics integration
  - Body creation and management
  - Map border creation
  - Physics stepping at 60Hz

- **LeaderBoard.cs** - Player ranking system

#### Server Entry Points
- **Program.cs** - HTTP server setup, Kestrel configuration on port 5000
  - HTTP/1, HTTP/2, HTTP/3 support
  - Global exception handler
  
- **Startup.cs** - WebSocket configuration
  - `/ws` endpoint for game connections
  - Binary packet receive loop
  - Malformed packet detection
  - Graceful disconnection handling

---

## Client Architecture (TypeScript - PixiJS)

### Core ECS (`pixiejsClient/src/app/ecs/`)

#### ECS Framework (`src/app/ecs/core/`)
- **NTT.ts** - Entity handle with GUID
  - Component methods: `set<T>()`, `get<T>()`, `has<T>()`
  
- **Component.ts** - Base component class
  - Registry system for component lookup
  - Type-safe component management
  
- **System.ts** - Base system class
  - Entity filtering by component query
  - `initialize()` lifecycle method
  - `update(entities: NTT[])` abstract method
  
- **World.ts** - Central ECS coordinator
  - Entity lifecycle management
  - System registration and update
  - Component query system
  - Entity changed notifications
  - Tick counter
  
- **Camera.ts** - Camera transform (position, zoom)

- **types.ts** - Shared type definitions

#### Components (`src/app/ecs/components/`) - 30 Components

**Physics & Network:**
- `PhysicsComponent.ts` - Position, velocity, rotation, shape
- `NetworkComponent.ts` - Server sync state tracking
- `ParentChildComponent.ts` - Ship part hierarchy

**Health & Damage:**
- `HealthComponent.ts` - Current/max health
- `HealthRegenComponent.ts` - Health regen rate
- `BodyDamageComponent.ts` - Body part damage
- `DamageComponent.ts` - Damage values
- `ShieldComponent.ts` - Shield absorption

**Energy & Systems:**
- `EnergyComponent.ts` - Power system state
- `EngineComponent.ts` - Engine thrust
- `WeaponComponent.ts` - Weapon state

**Gameplay:**
- `InputComponent.ts` - Player input
- `LevelComponent.ts` - Level and experience
- `InventoryComponent.ts` - Resource inventory
- `BulletComponent.ts` - Projectile
- `PickableTagComponent.ts` - Marker for pickable items
- `SpawnerComponent.ts` - Spawner configuration

**Rendering:**
- `RenderComponent.ts` - Render shape and color
- `ColorComponent.ts` - RGB color
- `EffectComponent.ts` - Visual effects

**State:**
- `DeathTagComponent.ts` - Death marker
- `RespawnTagComponent.ts` - Respawn marker
- `HoverTagComponent.ts` - UI hover state
- `LifeTimeComponent.ts` - Entity lifetime
- `ExpRewardComponent.ts` - Experience reward
- `GravityComponent.ts` - Gravity source
- `LineComponent.ts` - Line rendering
- `ParticleSystemComponent.ts` - Particle effects

#### Systems (`src/app/ecs/systems/`)
- **DeathSystem.ts** - Handle entity death cleanup
- **HealthDamageSystem.ts** - Apply damage and process health
- **LifetimeSystem.ts** - Remove expired entities
- **ParticleSystem.ts** - Update particle effects

**Renderers** (`src/app/ecs/systems/renderers/`):
- **BaseRenderer.ts** - Base rendering class
- **EntityRenderer.ts** - Render ship entities
- **ShieldRenderer.ts** - Render shield bubbles
- **ParticleRenderer.ts** - Render particles
- **EffectRenderer.ts** - Render visual effects
- **LineRenderer.ts** - Render line elements

#### Effects (`src/app/ecs/effects/`)
- **ImpactParticleManager.ts** - Create impact particle effects

### Network (`src/app/network/`)

#### Network Manager
- **NetworkManager.ts** - Main network coordinator
  - WebSocket connection management
  - Packet sending/receiving
  - Automatic reconnection
  - Server URL configuration
  
- **PacketHandler.ts** - Packet routing
  - Route packets by ID
  - Call appropriate handlers
  
- **PacketHeader.ts** - Packet header format
- **EvPacketReader.ts** - Binary deserialization
- **EvPacketWriter.ts** - Binary serialization

#### Packets (`src/app/network/packets/`) - 6 Packet Types
- **LoginRequestPacket.ts** - Player login with username
- **LoginResponsePacket.ts** - Server response with spawn info
- **ComponentStatePacket.ts** - Component sync with side effects
- **ChatPacket.ts** - Chat messages
- **PingPacket.ts** - Keepalive
- **LineSpawnPacket.ts** - Line element creation

### Managers (`src/app/managers/`) - 12 Managers

**Game Managers:**
- **GameConnectionManager.ts** - Connection state and lifecycle
- **GameInputHandler.ts** - Input capture and processing
- **GameUIManager.ts** - UI overlay management
- **InputManager.ts** - Keyboard/mouse input handling
- **KeybindManager.ts** - Keybinding management

**Game Systems:**
- **CameraManager.ts** - Camera positioning and zoom
- **InputManager.ts** - Game input state
- **ShipPartManager.ts** - Ship part rendering and updates
- **PlayerNameManager.ts** - Player name display

**Build Mode:**
- **BuildModeManager.ts** - Ship building UI
- **BuildModeController.ts** - Building logic

**Utilities:**
- **BackgroundRenderer.ts** - Parallax background
- **PerformanceMonitor.ts** - FPS and timing metrics

### UI (`src/app/ui/`)

**Game UI** (`src/app/ui/game/`):
- **ChatBox.ts** - Chat display and input
- **PlayerBars.ts** - Health and shield bars
- **TargetBars.ts** - Target health display
- **StatsPanel.ts** - Player stats panel
- **ShipStatsDisplay.ts** - Ship statistics
- **PerformanceDisplay.ts** - FPS counter
- **InputDisplay.ts** - Input visualization
- **PauseMenu.ts** - Pause menu
- **SettingsPage.ts** - Settings UI
- **SectorMap.ts** - Map display

**Ship Builder UI** (`src/app/ui/shipbuilder/`):
- **BuildGrid.ts** - Grid layout for ship building
- **ComponentDialog.ts** - Component selection dialog
- **ShapeSelector.ts** - Shape selection UI

**Base UI Components:**
- **Button.ts** - Clickable button
- **Checkbox.ts** - Toggle checkbox
- **Label.ts** - Text label
- **RoundedBox.ts** - Rounded rectangle box
- **VolumeSlider.ts** - Volume control slider

### Screens (`src/app/screens/`)
- **GameScreen.ts** - Main game play screen
  - Initializes all systems and managers
  - World container setup
  - Game loop integration

### Enumerations (`src/app/enums/`)
- **ComponentIds.ts** - ServerComponentType (43 types, matches server)
- **EffectType.ts** - Visual effect types

### Utilities (`src/app/utils/`)
- **userSettings.ts** - Persistent user preferences

### Engine Framework (`src/engine/`)

**Main Engine:**
- **engine.ts** - Core game engine (extends PixiJS)
  - Application initialization
  - Plugin system integration
  - Screen/navigation management

**Plugins:**
- **AudioPlugin.ts** - @pixi/sound integration
- **ResizePlugin.ts** - Window resize handling
- **NavigationPlugin.ts** - Screen navigation

**Systems:**
- **audio/audio.ts** - Audio playback
- **navigation/navigation.ts** - Screen transitions
- **resize/resize.ts** - Canvas resizing

**Utilities:**
- **utils/getResolution.ts** - Device resolution detection
- **utils/storage.ts** - Local storage wrapper

### Entry Point
- **main.ts** - Application initialization
  - Engine creation
  - GameScreen setup
  - Settings initialization

---

## Component Reference

### Component Synchronization
- **Server → Client**: ComponentStatePacket (Component ID + binary data)
- **Serialization**: StructLayout(Sequential, Pack=1) for deterministic binary format
- **Sync Flag**: Components marked with `[Component(NetworkSync=true)]`

### Common Component Properties
All networked components include:
- `long ChangedTick` - Server tick when component changed (used for sync)

---

## Networking Protocol

### Packet Structure
```
[2 bytes: Size]
[2 bytes: Packet ID]
[Variable: Payload]
```

### Packet Types
| ID | Name | Direction | Purpose |
|----|------|-----------|---------|
| 1 | LoginRequest | C→S | Player login with username |
| 2 | LoginResponse | S→C | Server response with map bounds |
| 10 | ChatPacket | Bi | Chat message |
| 21 | InputPacket | C→S | Player input state |
| 30 | PresetSpawnPacket | C→S | Spawn preset entity |
| 31 | CustomSpawnPacket | C→S | Spawn custom entity |
| 33 | LineSpawnPacket | S→C | Create line element |
| 39 | RequestSpawnPacket | C→S | Request entity spawn |
| 50 | ComponentState | S→C | Component sync |
| 90 | Ping | Bi | Keepalive |

### Network Optimization
- **Viewport Culling**: ViewportSystem only sends entities within player viewport
- **Component Sync**: Only changed components sent (ChangedTick comparison)
- **Entity Lifecycle**: Entities created/destroyed via packets

---

## Build Configuration

### Server (.NET 9)
**Project File**: `server/server.csproj`
- **SDK**: Microsoft.NET.Sdk.Web
- **Framework**: net9
- **AllowUnsafeBlocks**: true (for performance)
- **EnablePreviewFeatures**: true

**Dependencies**:
- Auios.QuadTree (v1.1.1)
- Box2D.NET (v3.1.1.557)

**Build Commands**:
```bash
dotnet build server/server.csproj              # Debug build
dotnet build server/server.csproj -c Release   # Release build
cd server && dotnet run                        # Run server
```

### Client (TypeScript/PixiJS)
**Build System**: Vite
**Package Manager**: npm

**Dependencies**:
- pixi.js (v8.8.1)
- @pixi/sound (v6.0.1)
- @pixi/ui (v2.2.2)
- box2d-wasm (v7.0.0)
- motion (v12.4.7)
- @esotericsoftware/spine-pixi-v8 (v4.2.74)

**Dev Dependencies**:
- typescript (~5.7.3)
- eslint, prettier (linting)
- vite (v6.2.0)
- @assetpack/core (v1.4.0)

**Build Commands**:
```bash
cd pixiejsClient
npm run dev           # Development server (port 8080)
npm run build         # Production build
npm run type-check    # TypeScript type checking
npm run lint          # ESLint check
```

### VSCode Configuration
**Launch Configs** (`launch.json`):
- SERVER - Debug .NET server
- FRONTEND - Run Vite dev server
- Debug PixiJS Client - Chrome debugging
- Attach to PixiJS Client - Remote debugging

**Compounds**:
- SERVER + FRONTEND - Both
- SERVER ONLY - Just server
- SERVER + FRONTEND + DEBUG - Full debugging setup

**Build Tasks** (`tasks.json`):
- build - dotnet build
- frontend-dev - npm run dev (background)

---

## Physics & Coordinate System

### Box2D Coordinate System
**CRITICAL**: Axes configuration in this game:
- **Positive Y = DOWN, Negative Y = UP** (gravity = +9.81 in Y)
- **Positive X = RIGHT, Negative X = LEFT**
- **Rotation**:
  - 0° = pointing RIGHT (positive X)
  - +90° = pointing DOWN (positive Y)
  - -90° = pointing UP (negative Y)
  - +180° = pointing LEFT (negative X)

### Force Application
- Forward direction: `Vector2(cos(rotation), sin(rotation))`
- Upward thrust: Use negative Y force to counteract gravity
- Spawn pointing UP: Use -90° rotation

### Shape Types
- **Circle**: ShapeType.Circle
- **Box**: ShapeType.Box
- **Polygon**: Multiple sides (3-8)

### Collision Categories
- **Player**: CollisionCategory.Player
- **Asteroid**: CollisionCategory.Asteroid
- **Projectile**: CollisionCategory.Projectile
- **Pickup**: CollisionCategory.Pickup

---

## Key Classes and File Locations

### Server Key Classes
| Class | File | Purpose |
|-------|------|---------|
| NTT | `/server/NttECS/ECS/NTT.cs` | Entity handle |
| NttWorld | `/server/NttECS/ECS/NttWorld.cs` | ECS coordinator |
| NttSystem | `/server/NttECS/ECS/NttSystem.cs` | System base |
| Game | `/server/Simulation/Game.cs` | Game coordinator |
| PhysicsWorld | `/server/Simulation/PhysicsWorld.cs` | Box2D wrapper |
| PacketHandler | `/server/Simulation/Net/PacketHandler.cs` | Packet routing |
| ComponentSerializer | `/server/Serialization/ComponentSerializer.cs` | Serialization |
| SpawnManager | `/server/Simulation/Managers/SpawnManager.cs` | Entity spawning |

### Client Key Classes
| Class | File | Purpose |
|-------|------|---------|
| NTT | `/pixiejsClient/src/app/ecs/core/NTT.ts` | Entity handle |
| World | `/pixiejsClient/src/app/ecs/core/World.ts` | ECS coordinator |
| Component | `/pixiejsClient/src/app/ecs/core/Component.ts` | Component base |
| System | `/pixiejsClient/src/app/ecs/core/System.ts` | System base |
| GameScreen | `/pixiejsClient/src/app/screens/game/GameScreen.ts` | Main screen |
| NetworkManager | `/pixiejsClient/src/app/network/NetworkManager.ts` | Network |
| CreationEngine | `/pixiejsClient/src/engine/engine.ts` | PixiJS wrapper |

---

## Code Metrics

| Aspect | Count |
|--------|-------|
| Server Components | 28 |
| Server Systems | 21 |
| Client Components | 30 |
| Client Systems | 4 (core) + 6 (renderers) |
| Client Managers | 12 |
| Packet Types | 6+ |
| Component Types (enum) | 43 |
| Total Server LOC | ~2,000+ |
| Total Client LOC | ~7,000+ |

---

## Performance Features

### Server
- **Struct-of-arrays storage** for cache efficiency and SIMD
- **Lock-free pooling** with Interlocked operations
- **Multi-threaded systems** - configurable thread count per system
- **Viewport culling** - only sync visible entities
- **Delta time** based physics (not fixed step all)
- **GC optimization** - SustainedLowLatency mode

### Client
- **Direct server position application** - no client prediction
- **Visual interpolation** - graphics lerp toward physics (60 FPS rendering)
- **PixiJS WebGL** rendering with antialiasing support
- **Viewport-based rendering** culling
- **Asset pack** optimization with cache busting

---

## Development Workflow

### Adding a New Component
1. Create struct in `server/Simulation/Components/`
2. Apply `[Component(ComponentType=..., NetworkSync=true/false)]`
3. Add ComponentType to `server/Enums/ComponentIds.cs`
4. Create matching TypeScript class in `pixiejsClient/src/app/ecs/components/`
5. Add to ComponentType enum in `pixiejsClient/src/app/enums/ComponentIds.ts`
6. Add deserialization case in `ComponentStatePacket.ts`

### Adding a New System
1. Create class inheriting `NttSystem` in `server/Simulation/Systems/`
2. Override `Update(start, count)` method
3. Manually register in `Game.cs` systems list (order critical!)
4. Match with client-side system in `pixiejsClient/src/app/ecs/systems/`

### Adding a New Packet Type
1. Define packet class in `pixiejsClient/src/app/network/packets/`
2. Add PacketId to `pixiejsClient/src/app/network/PacketIds.ts`
3. Implement handler in `server/Simulation/Net/PacketHandler.cs`
4. Add client handler in appropriate manager

---

## Important Notes

- **No braces on single-line if statements** (per code conventions)
- **File-scoped namespaces** (namespace X;)
- **Unsafe code enabled** for performance paths
- **Nullable reference types disabled** for compatibility
- **Server runs at 60 TPS** (ticks per second)
- **Physics at 60 Hz** separate from game logic
- **Client renders at ~60 FPS** with visual interpolation
- **Server is fully authoritative** - client trusts server state
- **Map bounds**: 32,000 x 32,000 units

---

## File Statistics

```
Server source files:      ~50 .cs files
Server LOC:              ~2,000 lines
Server components LOC:   ~644 lines
Server systems LOC:      ~1,221 lines

Client source files:     ~105 .ts files
Client LOC:              ~7,000+ lines
Client ECS files:        ~45 files
Client managers:         ~12 files
Client UI files:         ~20+ files
```

