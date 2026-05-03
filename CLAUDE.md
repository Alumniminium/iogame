# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Reference

**Repository**: IOGame — physics-based multiplayer space sandbox (browser).
**Server**: .NET 9 ASP.NET Core + custom ECS framework, Box2D.NET physics, WebSocket on port 5000.
**Client**: TypeScript + PixiJS 8.8.1 + Vite (dev server on 8080), client-side ECS that mirrors the server.
**Detailed file index**: `CODEBASE_INDEX.md` — refer to it instead of duplicating listings here.
**Component sync deep-dive**: `COMPONENT_SYNC_TECHNICAL_SPEC.md`.

## Build and Development Commands

```bash
# Server
dotnet build server/server.csproj         # Build server only
dotnet build iogame.sln                   # Build whole solution
cd server && dotnet run                   # Run server (Kestrel, port 5000, /ws WebSocket)

# Client (pixiejsClient/)
npm run dev          # Vite dev server on port 8080
npm run build        # lint:fix → type-check → vite build
npm run type-check   # tsc --noEmit
npm run lint         # eslint .
npm run lint:fix     # eslint . --fix
```

VSCode debugging: use the "SERVER" launch config in `.vscode/launch.json`.

**Do NOT run the server or frontend yourself** — both are long-running processes that don't exit. Ask the user to run/test when needed.

There are no automated tests in this repo.

## Architecture Overview

### Custom ECS framework (`server/NttECS/`)

- **NTT** (`NttECS/ECS/NTT.cs`) — entity handle (Guid). Generic API: `Set<T>()`, `Get<T>()`, `Has<T>()`, `Remove<T>()`, plus multi-arg `Has<T1, T2, ...>()`.
- **NttWorld** (`NttECS/ECS/NttWorld.cs`) — central coordinator: entity lifecycle, systems list, parent/child index, tick counter, JSON persistence to `_STATE_FILES/NttWorld.json` + `tick.last`.
- **NttSystem<T1, T2, ...>** (`NttECS/ECS/NttSystem.cs`) — base class. Override `Update(start, count)`; can opt into multi-threading via `BeginUpdate()`/`EndUpdate()` and a `threads` parameter.
- **NttQuery** (`NttECS/ECS/NttQuery.cs`) — for ad-hoc queries outside the system loop (e.g. broadcasting all gravity sources to a newly-logged-in player). Don't use this inside a `NttSystem.Update` — those iterate via `Entities`.
- **PackedComponentStorage** — struct-of-arrays storage; SIMD- and cache-friendly.
- **Pool<T>** / **SwapList<T>** (`NttECS/Memory/`) — lock-free pool, SIMD-vectorized list (8x–16x faster `Contains`/`IndexOf`).
- **MultiThreadWorkQueue** / **ThreadedWorker** — dedicated worker threads for partitioned system updates.

### Game loop (`server/Simulation/Game.cs`)

The game loop **decouples physics from system updates**:

- Physics steps at a fixed **60 Hz** (`PhysicsWorld.Step(1/60)`) using its own accumulator, may step multiple times per outer loop iteration.
- Systems update at the **target TPS (60)** via `NttWorld.UpdateSystems()`, sandwiched between `IncomingPacketQueue.ProcessAll()` and `PacketQueue.FlushAll()`.

**System execution order (do not change without understanding the dependencies):**

```
SpawnSystem → ViewportSystem → InputSystem
  → PositionSyncSystem → ShipPhysicsRebuildSystem
  → GravitySystem → EngineSystem → EnergySystem → ShieldSystem → WeaponSystem
  → PickupCollisionResolver → ProjectileCollisionSystem → DamageSystem → HealthSystem → DropSystem
  → LifetimeSystem → LevelExpSystem → RespawnSystem
  → ComponentSyncSystem → DeathSystem
```

`CollisionSystem` runs inside the Box2D step (collision callbacks), not in this list. `ComponentSyncSystem` must run after gameplay systems mutate state and before `DeathSystem` destroys entities.

### Networking

- WebSocket at `/ws` (server port 5000). Wire format: `[u16 size][u16 packetId][payload]`. Multiple packets may be coalesced in one WebSocket message — the client parses them in a loop (see `pixiejsClient/src/app/network/PacketHandler.ts`).
- **Server-authoritative**, no client prediction. Client renders server state and visually lerps graphics toward the latest physics position.
- **Component-based delta sync**: `ComponentSyncSystem` serializes only components whose `ChangedTick == NttWorld.Tick` and only to clients whose viewport contains the entity (`ViewportSystem` pre-computes visibility).
- **Player input is sent as a `ComponentStatePacket`** (an `InputComponent` mutation) over PacketId 50, *not* a dedicated input packet. The legacy `InputPacket` ID (21) still exists in the enum but the client no longer emits it.
- Packet IDs live in `server/Enums/PacketId.cs` and `pixiejsClient/src/app/network/PacketHandler.ts` — keep them in sync.

### Client architecture (`pixiejsClient/src/app/`)

- `ecs/core/` — `World`, `NTT`, `Component`, `System`. Mirrors the server API but in TypeScript.
- `ecs/components/` — class-based components, mirror server structs (plus client-only ones like `HoverTagComponent`, `LineComponent`, `ParticleSystemComponent`, `RenderComponent`, `ShipPartComponent`).
- `ecs/systems/` — gameplay-ish client systems (`HealthDamageSystem`, `LifetimeSystem`, `ParticleSystem`, `DeathSystem`).
- `ecs/systems/renderers/` — PixiJS rendering (`EntityRenderer`, `ShieldRenderer`, `ParticleRenderer`, `EffectRenderer`, `LineRenderer`, base `BaseRenderer`).
- `managers/` — non-ECS coordinators (network connection, input, camera, build mode, ship parts, performance, parallax background, etc.). New systems/managers register inside `screens/game/GameScreen.ts::initializeGame`.
- `network/packets/` — one file per packet type with `serialize`/`deserialize`/`handle` methods.
- `theme/colors.ts` — central UI palette; prefer importing from here over hardcoding hex literals in UI files.

## Conventions and Critical Rules

### Box2D coordinate system

- **+Y is DOWN, −Y is UP** (gravity = +9.81 Y). +X right, −X left.
- **Rotation 0 = pointing +X (right)**. −π/2 = up. +π/2 = down. ±π = left.
- Forward vector: `new Vector2(MathF.Cos(rotation), MathF.Sin(rotation))`.
- To spawn pointing up, rotate to `−MathF.PI / 2f` (see `PacketHandler.cs` LoginRequest handler).
- Box2D works in **density**, not mass: `density = desiredMass / (width × height)`.

### Components

A component is a `struct` (server) / `class` (client) with mirrored fields.

Server checklist for a new networked component:
1. `[Component(ComponentType = ComponentType.X, NetworkSync = true)]` + `[StructLayout(LayoutKind.Sequential, Pack = 1)]`.
2. **First field MUST be `public long ChangedTick;`** — `ComponentSerializer` reads raw bytes and assumes this layout.
3. Add the enum value in `server/Enums/ComponentIds.cs` and the matching one in `pixiejsClient/src/app/enums/ComponentIds.ts`.
4. Add a deserialization branch in `pixiejsClient/src/app/network/packets/ComponentStatePacket.ts`.

Note: `Simulation/Components/Box2DBodyComponent.cs` defines a struct named `PhysicsComponent` (file name is historical — the struct is what's referenced everywhere). Some "components" share a file with related ones (e.g. `DropResourceComponent` lives in `PickupComponent.cs`).

### Systems

Server: subclass `NttSystem<...>`, override `Update(start, count)`, **register in `Game.cs`'s `systems` list — order matters** (see above).
Client: subclass `System`, override `update(entities)`, register in `GameScreen.ts`'s `World.setSystems(...)` call.

### Entity spawning

Use `server/Simulation/Managers/SpawnManager.cs` whenever possible. Player spawns are still inline in `PacketHandler.cs` (LoginRequest handler) — a good template if you need to mint a new ad-hoc entity.

### C# style

- File-scoped namespaces (`namespace server.X;`).
- Single-line `if` without braces is the house style.
- `unsafe` is enabled for hot paths; nullable refs are off.
- `.NET 9` with preview language features.
- `.editorconfig` hides several diagnostics (`CA1725`, `IDE0008`, `IDE0011`, `IDE0058`, `CA1051`).

### Performance baseline

- Server GC mode: `SustainedLowLatency`.
- Map: 32,000 × 32,000 units, gravity sources at top/bottom edges.
- Tick rate: 60 TPS (systems), 60 Hz (physics, separate accumulator), client renders ~60 FPS.

## Dependencies

- **Server**: Box2D.NET 3.1.1.557, Auios.QuadTree 1.1.1.
- **Client**: pixi.js 8.8.1, @pixi/sound 6, @pixi/ui 2.2, box2d-wasm 7, @esotericsoftware/spine-pixi-v8 4.2, motion 12, TypeScript 5.7, Vite 6.
