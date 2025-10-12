# PixiJS Client Architecture

## Directory Structure

```
src/
├── main.ts              # Entry point, initializes engine and GameScreen
├── engine/              # Custom PixiJS engine wrapper (CreationEngine)
├── app/
│   ├── ecs/            # Entity Component System
│   │   ├── core/       # World, Entity, Component, System base classes
│   │   ├── components/ # Component definitions (PhysicsComponent, Velocity, Health, etc.)
│   │   ├── systems/    # Game systems (Input, Network, Render, Particle, etc.)
│   │   └── effects/    # Visual effects (particles, impact effects)
│   ├── network/        # NetworkManager and packet handlers
│   │   └── packets/    # Packet definitions (ComponentState, EntitySync, etc.)
│   ├── managers/       # Game managers (Input, PlayerName, ShipPart, etc.)
│   ├── screens/        # Screen management (GameScreen)
│   ├── ui/             # UI components (HUD, StatsPanel, Chat, etc.)
│   ├── enums/          # Shared enumerations (ComponentType, PacketId, etc.)
│   └── utils/          # Utility functions
└── public/             # Static assets
```

## Core Architecture Patterns

### Client-Side ECS
- **World** - Central coordinator managing entities and systems
- **Entity** - Lightweight containers with unique IDs and component storage
- **Component** - Data-only classes (PhysicsComponent, Velocity, Health, Shield, etc.)
- **System** - Logic processors that update entities each frame

### Key Component: PhysicsComponent
**PhysicsComponent** (formerly Box2DBodyComponent) stores server-synced physics state:
- Position, rotation, velocity
- Size, shape, color
- Last known server position/rotation for interpolation
- Synced from server's PhysicsComponent via ComponentStatePacket

### System Execution Order
Systems execute each frame in this order:
1. **InputSystem** - Capture and send player input to server
2. **NetworkSystem** - Process incoming packets, sync entity state
3. **BuildModeSystem** - Handle ship building interface
4. **ShipPartSyncSystem** - Sync ship part visual data
5. **ParticleSystem** - Update particle effects
6. **LifetimeSystem** - Remove expired entities
7. **RenderSystem** - Render all entities with camera transforms

### Network Architecture
- **Fully server-authoritative** - Server runs physics at 60 TPS
- **No client prediction** - Client applies server positions directly
- **Visual interpolation** - Graphics lerp toward physics positions for 60 FPS smoothness
- **Component-based sync** - Only changed components sent from server
- **Viewport culling** - Only entities in viewport are synced

### Rendering Pattern
- Server sends authoritative positions via PhysicsComponent updates
- NetworkSystem directly updates entity PhysicsComponent data
- RenderSystem visually lerps graphics for smooth 60 FPS rendering
- Camera system with pan/zoom controls

## Graphics Management
All Graphics objects are centralized in **RenderComponent.renderers** Map:
- Key: Component constructor (type-safe)
- Value: Graphics object
- Survives network updates (RenderComponent persists client-side)
- Cleaned up by DeathSystem when entity destroyed