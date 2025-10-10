# Project Overview

## Purpose
This is a **multiplayer IO game server** built with .NET 9 ASP.NET Core. It provides a real-time multiplayer game server using WebSockets with a custom high-performance Entity Component System (ECS) architecture.

## Tech Stack
- **.NET 9** (net9) - ASP.NET Core Web Application
- **Box2D.NET** (v3.1.1.557) - Physics engine for game simulation
- **Auios.QuadTree** (v1.1.1) - Spatial partitioning for efficient collision detection
- **WebSockets** - Real-time client-server communication
- **Custom ECS Framework** - High-performance entity component system

## Project Structure
```
server/
├── NttECS/               # Core ECS framework
│   ├── ECS/             # NTT, NttWorld, NttSystem, PackedComponentStorage
│   ├── Memory/          # Lock-free pooling and memory management
│   ├── Threading/       # Multi-threaded work queues
│   └── Utilities/       # SIMD-optimized data structures (SwapList)
├── Simulation/          # Game-specific implementation
│   ├── Components/      # Data-only component structs
│   ├── Systems/         # Game logic systems (21 systems)
│   ├── Managers/        # SpawnManager, etc.
│   ├── Net/            # PacketHandler for client communication
│   └── Database/        # Persistence layer
├── Serialization/       # Component serialization for network sync
├── Helpers/            # Utilities (Vector2Ext, PerformanceMetrics)
├── Enums/              # Shared enumerations (ComponentType, etc.)
├── Program.cs          # Application entry point
└── Startup.cs          # ASP.NET Core configuration
```

## Architecture Highlights
- **Custom ECS**: High-performance entity component system with struct-of-arrays storage
- **Server-authoritative**: All game logic and physics run on server at 60 TPS
- **Multi-threaded**: Systems can process entities across multiple threads
- **Zero-allocation hot paths**: Lock-free pooling and SIMD optimizations
- **Component-based networking**: Only changed components synced to clients
- **Viewport culling**: Clients only receive data for entities in their view