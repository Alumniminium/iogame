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

**Web client development:**
The WebClient is pure JavaScript - serve `WebClient/index.html` with any web server. No build process required.

## Architecture Overview

This is a multiplayer IO game with a custom **Entity Component System (ECS)** architecture:

### Project Structure
- **server/** - .NET 9 ASP.NET Core server with custom ECS game engine
- **WebClient/** - JavaScript HTML5 Canvas client  
- **Shared/** - Common packet definitions and utilities
- **FNAClient/** - Deleted desktop client (remnants in git history)

### Core ECS Architecture (server/ECS/)
- **PixelEntity** - Lightweight entity structs with parent-child relationships
- **PixelWorld** - Central ECS coordinator managing 500K+ entities efficiently  
- **Components** - Data-only structs in server/Simulation/Components/
- **Systems** - Logic processors in server/Simulation/Systems/

### Game Loop Order (critical for modifications)
1. SpawnSystem - Entity creation
2. InputSystem - Player input processing
3. PhysicsSystem - Movement and physics  
4. Collision systems (AABB broad phase → narrow phase)
5. Gameplay systems (weapons, damage, health, shields)
6. NetworkSystems - State synchronization

### Networking Architecture
- **WebSocket-based** real-time communication via `/ws` endpoint
- **Binary packet protocol** defined in Shared/ project
- **Server authority** with client prediction for smooth gameplay
- **Viewport culling** - clients only receive nearby entity data

### Key Performance Patterns
- **Array-of-structures** component storage for cache efficiency
- **Object pooling** throughout for GC pressure reduction
- **Spatial partitioning** using Grid + QuadTree hybrid
- **Parent-child transforms** for complex multi-part entities

## Common Development Patterns

**Adding new components:**
1. Create data-only struct in server/Simulation/Components/
2. Add [Component] attribute  
3. Components auto-register with ECS

**Adding new systems:**
1. Inherit from PixelSystem in server/ECS/
2. Override ProcessEntity with component type constraints
3. Systems auto-register and run in dependency order

**Adding new packet types:**
1. Define packet struct in Shared/ project
2. Add to PacketId enum
3. Implement handler in server/Simulation/Net/PacketHandler.cs
4. Add client-side handling in WebClient/js/network/

**Entity spawning:**
Use SpawnManager.cs for consistent entity creation with proper component initialization.

## Code Conventions

- Use file-scoped namespaces: `namespace MyNamespace;`
- No braces for single-line if statements
- Unsafe code enabled for performance-critical paths
- .NET 9 with preview language features
- Nullable reference types disabled for compatibility
- dont run the frontend or server. the processes do not exit and only the user can test. ask him to run and test it when needed.