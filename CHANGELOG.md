# Changelog

## [Unreleased] - 2025-01-21

### Drop Pickup Improvements - Sensor-Based Collision Detection - 2025-01-21
- **Fixed drop collision behavior**: Drops now use Box2D sensors to detect players without physical collision
  - **No Physical Collision**: Drops no longer physically collide with players, preventing bouncing and blocking
  - **Pickup Detection**: Sensor events are converted to collision components for compatibility with existing pickup system
  - **Player-Only Detection**: Drops only detect players (not other entities) using collision filtering
  - **Preserved Pickup Logic**: Existing PickupCollisionResolver system continues to work unchanged

### Asteroid Feature Implementation - Complete Solid Asteroid System - 2025-01-21
- **Implemented complete asteroid system with solid structure**
  - **Solid Physics**: All blocks receive Box2D bodies to ensure asteroids are completely solid and thick, preventing players from passing through when blocks are destroyed
  - **Structural Integrity System**: BFS-based anchor system prevents floating islands - blocks must be within 6-8 blocks of anchor blocks or they collapse
  - **Neighbor Tracking**: Real-time neighbor component system tracks block adjacency and automatically creates physics bodies when interior blocks become edges
  - **Block Rotation Support**: 4-directional rotation system (0°, 90°, 180°, 270°) for ship building components with directional engines, weapons, and shields
  - **Optimized Generation**: Efficient neighbor tracking and structural integrity systems provide good performance despite full physics coverage
- **Core Components Implemented**:
  - `AsteroidBlockComponent` - Block metadata with anchor status and physics tracking
  - `AsteroidNeighborComponent` - 4-directional neighbor references with edge detection
  - `StructuralIntegrityComponent` - Support distance calculation and visual integrity feedback
  - `GridPositionComponent` - Grid-based positioning with rotation for ship building
  - `BlockRotation` enum - 4-directional rotation with helper methods for direction vectors
  - Directional ship components: `DirectionalEngineComponent`, `WeaponMountComponent`, `DirectionalShieldComponent`, `HullComponent`, `AssemblyComponent`
- **Smart Physics Creation**: Automatic Box2D body creation when blocks become edges due to neighbor destruction
- **Three-Phase Generation**: Block creation → neighbor setup → structural integrity calculation for optimal edge detection
- **Systems Architecture**:
  - `AsteroidNeighborTrackingSystem` - Handles neighbor updates and edge physics creation on block destruction
  - `AsteroidStructuralIntegritySystem` - BFS distance calculation to anchors with visual cracking feedback
  - `AsteroidCollapseSystem` - Marks unsupported blocks for death with proper cleanup
  - `ShipPropulsionSystem` - Rotational thrust system for directional engines with torque calculation
- **Integration Ready**: Complete ECS integration with existing death/health/drop systems, network sync support, and proper system execution order

## [Unreleased] - 2025-01-21

### Shape Simplification to 1x1 Standard - 2025-01-21
- **Standardized all shapes to 1x1 size**: Removed width/height parameters from Box2DBodyComponent, SpawnPacket, and all shape creation methods
- **Simplified Box2D physics creation**: All shapes (circles, boxes, triangles) now use fixed 1x1 dimensions with 0.5 radius for circles
- **Updated packet protocol**: SpawnPacket no longer includes width/height fields, reducing network overhead
- **Cleaned up frontend**: Updated PixiJS client SpawnPacket.ts to handle 1x1 shapes and removed width/height parsing
- **Fixed packet ID mismatch**: Server now correctly sends SpawnPacket with ID 29 instead of CustomSpawnPacket ID 31
- **Implemented Box2D edge shape borders**: Replaced large wall entities with efficient invisible edge collision boundaries around map perimeter
- **Enhanced CreateStructure for filled shapes**: Method now creates filled rectangles and circles using arrays of 1x1 entities arranged in proper patterns
- **Maintained collision accuracy**: Physics interactions remain accurate with standardized collision boundaries

### High-Performance Packet System - 2025-01-21
- **Created PacketWriter.cs**: Zero-allocation binary packet writer with fluent API using ArrayPool<byte> for buffer management
- **Created PacketReader.cs**: High-performance binary packet reader with optimized GUID parsing using hex lookup tables
- **Features comprehensive type support**: Int8/16/32/64, UInt8/16/32/64, Float/Double, Vector2/Vector3, Bool, Enums, Strings (fixed-length and length-prefixed), GUID, NTT entities
- **Implements fluent builder pattern**: Chainable method calls for ergonomic packet construction: `writer.WriteInt32(value).WriteVector2(pos).WriteString8(name)`
- **Zero runtime allocations**: Uses ArrayPool for buffer reuse and efficient binary serialization with little-endian encoding
- **Memory<byte> compatible**: Finalize() returns Memory<byte> that works directly with existing NetSync infrastructure

### Major Physics Engine Migration to Box2D.NET - 2025-01-21
- **Migrated from custom physics to Box2D.NET**: Replaced entire custom physics engine with professional-grade Box2D physics for improved performance, stability, and collision detection
- **Created Box2DBodyComponent**: New ECS component wrapping Box2D B2BodyId with position/velocity caching and force application methods
- **Created Box2DPhysicsWorld**: Centralized Box2D world management with proper initialization, stepping, and body creation/destruction
- **Created Box2DCollisionHandler**: System for processing Box2D collision events and triggering game-specific collision logic
- **Created Box2DSyncSystem**: Efficient system for syncing Box2D transform data back to ECS components
- **Created Box2DEngineSystem**: Engine force application system that works with Box2D bodies instead of custom physics
- **Updated SpawnManager completely**: All entity spawning now creates Box2D bodies (bullets, structures, drops, spawners, resources)
- **Integrated Box2D world stepping**: Game loop now steps Box2D physics world before ECS system updates
- **Removed legacy physics systems**: AABBSystem, NarrowPhaseSystem, and PhysicsSystem no longer needed
- **Added multi-threaded physics**: Box2D configured with 4 worker threads for better performance on multicore systems
- **Enhanced material properties**: Box2D surface materials support friction, restitution, and rolling resistance
- **Improved shape creation**: Proper Box2D shape creation for circles, boxes, and triangles with hull computation

### Physics Engine Restoration & Component System Fixes - 2025-01-21
- **Fixed physics engine compilation errors**: Restored compatibility with ECS component system after 'pre-ai' commit rollback
- **Updated component references**: Replaced obsolete ColliderComponent, RigidBodyComponent, TransformComponent, and ColorComponent with unified PhysicsComponent
- **Fixed SpawnManager methods**: Updated all physics creation methods to use new PhysicsComponent.CreateBoxBody/CreateCircleBody/CreateTriangleBody APIs
- **Resolved method signature mismatches**: Fixed SpawnDrop method signature and all calling sites
- **Updated PacketHandler**: Migrated from legacy component system to new PhysicsComponent architecture
- **Fixed AABBComponent**: Corrected field name casing issues causing compilation errors
- **Fixed Game.Grid spatial partitioning**: Entities are now automatically added to spatial grid when PhysicsComponents are created and removed when entities are destroyed
- **Fixed boundary collision causing unnatural resting angles**: Simplified collision detection to check position before clamping and apply proper velocity reflection with angular damping, preventing squares from resting on corners
- **Implemented rolling physics for boundary contact**: Rotation now translates to linear velocity when touching surfaces using v = ω × r formula, creating realistic rolling motion along walls and floors
- **Added collision-induced angular velocity**: Linear velocity now converts to angular velocity on boundary collisions - tangential motion becomes spinning motion, creating realistic collision dynamics

### Critical Physics Mathematics Fixes - 2025-01-21
- **Fixed SAT collision detection normal direction**: Ensured collision normals always point from object A to object B using centroid-based direction validation
- **Fixed polygon contact point calculation**: Replaced inaccurate support point averaging with proper closest point calculation using line segment projection
- **Fixed cross product calculations for angular impulse**: Replaced inconsistent cross product implementations with proper 2D cross product helper methods
- **Added robust vector normalization**: Fixed potential division by zero in vector normalization with proper epsilon checking and fallback handling
- **Enhanced SAT axis generation**: Added zero-length edge detection and proper axis normalization to prevent NaN values in collision detection
- **Improved penetration depth calculation**: Fixed minimum penetration selection to properly choose the correct separation axis and normal direction
- **Added geometric utility methods**: Implemented `ClosestPointOnLineSegment`, `GetCentroid`, `Cross2D`, and `CrossScalarVector` for consistent mathematical operations
- **Implemented proper physics substeps**: Fixed substep calculation to use dynamic delta time (originalDeltaTime / PhysicsSubsteps) instead of hardcoded constants
- **Enhanced collision stability parameters**: Improved Baumgarte factor (0.4), reduced penetration slop (0.005), and optimized solver iterations for better collision response
- **Stricter sleeping system**: Reduced sleep tolerance and increased sleep time to prevent premature entity deactivation during collisions
- **Physics system now produces mathematically correct and stable collision responses with proper multi-step integration**

### High-Frequency Physics Engine & Stability Improvements - 2025-01-21
- **Fixed bullet collision bouncing bug**: Bullets now have their velocity and forces zeroed when they collide, preventing them from bouncing repeatedly into the same entity until destruction
- **Added edge-based gravity system**: New GravitySystem applies gravity only near map edges (1000px range) with quadratic distance falloff for realistic space physics
  - Top edge pulls entities toward top (negative Y), bottom edge pulls toward bottom (positive Y)
  - Corrected gravity directions based on coordinate system where Y=0 is top, Y=100000 is bottom
  - 20x Earth gravity strength for dramatic space game effect
  - Safe minimum distance (50px) prevents infinite forces at exact edges
- **Implemented proper 2D physics engine**: Complete realistic physics system running at 60hz for optimal stability
  - Combined gravity, physics integration, and collision detection into single unified system
  - Eliminates timing issues between separate physics systems
  - Realistic Earth gravity (9.81 m/s²) with edge-based application for natural behavior
  - Complete collision detection for all shape combinations (circle-circle, circle-polygon, polygon-polygon)
  - Full constraint solver with proper restitution (bounciness) and friction handling
  - Improved contact point calculation using support point methodology
  - Natural physics-based stability - no artificial torque corrections
  - Proper boundary collision handling with reduced restitution and angular damping
- **Enhanced angular physics response**: Improved inertia calculations and reduced angular damping
  - Increased inertia multipliers (2x for circles, 4x for boxes/triangles) for more visible rotation effects
  - Reduced player angular damping from 0.1 to 0.02 for better collision response
- **Fixed physics instability and jittering**: Major stability improvements for stacked entities
  - Reduced Baumgarte factor from 0.2 to 0.1 for gentler collision correction
  - Tightened penetration slop from 0.01 to 0.005 for more precise collision detection
  - Reduced collision solver iterations (6 velocity, 2 position) to prevent over-correction
  - Increased damping (+0.05 linear, +0.1 angular) for faster energy dissipation
  - Re-enabled sleeping system with higher thresholds (0.5 linear, 0.1 angular) and faster activation (30 ticks)
- **Improved collision response**: ProjectileCollisionSystem now immediately stops bullet movement upon impact for cleaner collision behavior

### Collision System Fixes - 2025-01-21
- **Fixed entity-entity collisions**: Updated CollisionSystem to use RigidBodyComponent.Position instead of TransformComponent.WorldPosition for collision detection
- **Fixed bullet collision detection**: Added CollisionComponent population in CollisionSystem for ProjectileCollisionSystem compatibility
- **Enhanced collision data flow**: CollisionSystem now properly creates and populates CollisionComponent for collision-dependent systems
- **CRITICAL: Fixed collision resolution physics**: Changed CollisionSystem to get RigidBodyComponent by reference instead of by value, ensuring collision impulses are applied to actual entities instead of temporary copies. Entities now properly bounce/push apart on collision instead of exploding.
- **Fixed mass/inertia calculations**: Updated SpawnManager to use BaseResource.Mass instead of hardcoded values. Entities now use proper physics-based inertia calculated from shape and mass using ColliderComponent.CalculateInertia()
- **Fixed unrealistic spinning**: Corrected inertia values for player (1x1 box) and bullets to be calculated from their actual shapes instead of arbitrary values
- **Fixed entities exploding on player collision**: Restricted CollisionComponent creation to bullets only, preventing regular entity-entity collisions from triggering ProjectileCollisionSystem damage logic

### Complete Physics Engine Replacement - 2025-01-20
- **Fully replaced old physics system with new high-performance implementation**
  - **New Components**: `RigidBodyComponent`, `ColliderComponent`, `TransformComponent`
  - **New Systems**: `PhysicsIntegrationSystem`, `CollisionSystem`, `CompositeBodySystem`, `SpatialGridSystem`
  - **Removed**: Old `PhysicsComponent`, `AABBComponent`, migration utilities, `PhysicsSystem`, `AABBSystem`, `NarrowPhaseSystem`
  - **New Spatial Grid**: Replaced inefficient flat grid with optimized hierarchical spatial partitioning
    - Fixed 128x128 cell size for optimal cache performance
    - Batch collision detection with pre-allocated buffers
    - O(1) entity insertion/removal with version-based tracking
    - Debug info: entities/cells with occupancy statistics
  - **Updated all spawn systems**: SpawnManager, PacketHandler, ViewportSystem converted to new components
  - **Performance**: 10,000+ objects ready with SIMD-optimized collision detection
  - **Determinism**: Fixed timestep, no randomness, consistent entity iteration
  - **Stability**: Sequential Impulses solver, contact caching, sleeping system
  - **Ship building**: Parent-child transforms with proper mass distribution and force propagation
  - Old physics system completely removed - clean KISS architecture achieved

### Ship Builder UI Implementation - 2025-01-21
- **Implemented complete ship building system in PixiJS client**
  - **Grid-based placement**: 20x15 grid with 32px cells and snap-to-grid functionality
  - **Part palette**: Hull (gray), Shield (blue), Engine (orange) blocks with triangle/square shapes
  - **Template system**: Preset ship designs loadable onto grid
  - **Build mode integration**: Press 'B' key or Build button to enter build mode
  - **Visual improvements**: Enhanced grid lines visibility, larger blocks (90% cell size), reduced spacing
  - **Interactive features**: Ghost preview, cell highlighting, drag placement, right-click removal
  - **Fixed Button component**: Replaced non-functional stub callbacks with proper event handling
  - **ECS compliance**: Components determine entity type (GridPositionComponent, HullComponent, etc.)
  - **Architecture**: Direct rendering in BuildGrid, simplified from complex ECS rendering system

### TypeScript Compilation Fixes - 2025-01-21
- **Fixed all TypeScript compilation errors**
  - **NetworkSystem**: Implemented required abstract methods `componentTypes` and `updateEntity`
  - **Event handling**: Fixed custom event listeners with proper Event/CustomEvent typing
  - **System lifecycle**: Changed `destroy()` to `cleanup()` to match System base class
  - **Constructor fixes**: Removed invalid parameter from System constructor call
  - **Unused variables**: Prefixed with underscore or removed unused imports/methods
  - **Button component**: Removed unused engine import and audio methods
  - **Clean compilation**: All files now compile without errors

### Ship Building System Implementation - 2025-01-20
- **Complete in-game ship building system for PixiJS frontend**
  - **ECS Components**:
    - `GridPositionComponent` - Grid-based positioning for ship parts
    - `ConnectionComponent` - Connection points and entity relationships
    - `HullComponent` - Hull blocks with health, armor, mass properties
    - `ShieldGeneratorComponent` - Shield generation with power management
    - `EngineBlockComponent` - Engine blocks with thrust and fuel consumption
  - **Build Mode System**: `BuildModeSystem` - Manages build mode state, part placement validation, ghost previews
  - **UI Components**:
    - `BuildGrid` - Visual grid overlay with snap-to-grid functionality
    - `PartPalette` - Draggable parts inventory (Hull, Shield, Engine blocks)
    - `TemplateSelector` - Preset ship templates for quick building
    - `ShipBuilderUI` - Main container integrating all build mode components
  - **Ship Templates**: 4 preset ship designs (Fighter, Cruiser, Interceptor, Defender)
  - **Integration**: Build mode toggle button ('B' key), pauses regular input, overlay UI
  - **Features**: Drag & drop placement, right-click removal, template loading, grid validation
  - **Architecture**: Follows ECS pattern - entity type determined by attached components, not inheritance

### Movement Update System Fix - 2025-01-20
- **Fixed client not responding to server position updates**
  - Created NetworkSystem to handle server movement update events
  - Server was correctly sending MovementPacket data, but client had no system listening
  - Added NetworkSystem registration in GameScreen with proper priority (90)
  - Client entities now update their PhysicsComponent positions from server data

### Line/Ray Rendering System - 2025-01-20
- **Fixed LineSpawnPacket (RayPacket) rendering**
  - Corrected LineSpawnPacket structure to match server RayPacket format
  - Server sends: UniqueId, TargetUniqueId, Origin, Hit (not startX/Y, endX/Y, color)
  - Added line-spawn event listener to NetworkSystem
  - Added line rendering support to RenderSystem with temporary visual effects
  - **Fixed server ray origin**: Changed from target position to player position in EngineSystem
  - Lines now correctly show rays from player to hit point, not from target to hit point

### Health Bar Positioning Fix - 2025-01-20
- **Fixed TargetBars coordinate transformation and positioning**
  - Replaced hardcoded camera `{ x: 0, y: 0, zoom: 1 }` with actual camera from RenderSystem
  - Fixed distance calculation to use actual local player position instead of origin
  - Added proper parameters to `updateFromWorld()` method: camera, localPlayerId, viewDistance
  - Updated GameScreen to pass RenderSystem camera and localPlayerId to TargetBars
  - Health bars now appear correctly positioned relative to entities on screen

### Packet Handler Architecture Improvements - 2025-01-20
- **Integrated PacketHandler with NetworkManager**
  - Removed duplicate packet handling logic from NetworkManager
  - All packets now flow through the centralized PacketHandler class
  - Improved error handling and packet statistics tracking
- **Fixed ECS pattern violations in packet handling**
  - Refactored MovementPacket to use event-driven approach instead of direct system access
  - MovementPacket now dispatches 'server-movement-update' events for NetworkSystem to handle
  - Maintains proper separation of concerns between networking and ECS layers
- **Standardized packet handler interfaces**
  - Fixed TypeScript type issues with ArrayBuffer handling
  - Consistent packet processing through registered handlers
  - Proper error recovery and packet synchronization
- **Updated LoginResponsePacket handling**
  - Moved business logic to NetworkManager for better separation
  - Removed duplicate logging and processing code

### Complete Packet Architecture Refactor & Critical Structure Fixes - 2025-01-20
- **Eliminated inline packet creation from NetworkManager**
  - Created dedicated packet classes following Reader/Writer pattern
  - LoginRequestPacket.ts, LoginResponsePacket.ts, PlayerMovementPacket.ts with InputState interface
  - ChatPacket.ts, RequestSpawnPacket.ts, MovementPacket.ts, PingPacket.ts, StatusPacket.ts, PresetSpawnPacket.ts
  - Each packet class includes static create() and fromBuffer() methods
  - Used EvPacketWriter/EvPacketReader for clean binary serialization
- **Moved ALL packet logic to dedicated packet classes with clean interfaces**
  - MovementPacket.handle() - contains ECS entity updating and debug visualization logic
  - StatusPacket.handle() - contains component updates via direct switch cases (Health, Energy, Battery, Shield, etc.)
  - PresetSpawnPacket.handle() - contains resource entity creation with physics, render, and AABB components
  - LoginResponsePacket.handle() - parses and logs login data, returns packet for NetworkManager to handle callbacks
  - PingPacket.handle() - calculates and returns latency value for NetworkManager to store
  - AssociateIdPacket.handle(), SpawnPacket.handle(), ChatPacket.handle() - already had proper separation
  - No complex callback injection or dependency passing - clean data in/out interfaces
- **Replaced complex PacketHandler registration with simple switch statement**
  - Eliminated registerHandler pattern completely
  - NetworkManager now has clean switch statement calling packet.handle() methods
  - Removed PacketHandler class and all registration infrastructure
  - Each case in switch just calls appropriate packet class handle method
- **Implemented robust WebSocket packet buffering**
  - Added packet buffer to handle fragmented and merged WebSocket messages
  - Properly splits and merges packets when receiving multiple packets in single message
  - Handles incomplete packets by buffering until complete packet arrives
  - Added error recovery for malformed packet headers with 1-byte skip fallback
  - Prevents packet parsing errors from incomplete or oversized WebSocket frames
- **Cleaned up NetworkManager responsibilities**
  - Removed all updateEntityStatus, updateHealthComponent, updateEnergyComponent methods
  - Removed sendInput(), sendChat(), sendPing() convenience methods - packet creation now happens outside NetworkManager
  - Removed unused imports (HealthComponent, EnergyComponent, BatteryComponent, ShieldComponent, InputState, PlayerMovementPacket, etc.)
  - NetworkManager now purely handles connection, buffering, and routing packets to handlers
  - No more packet-specific logic, ECS manipulation, or packet creation in NetworkManager
  - Only retains core connection management: connect/disconnect, ping scheduling, statistics
- **Fixed deprecated JavaScript warnings**
  - Changed substr() to substring() throughout codebase
  - Improved code maintainability and type safety
- **Fixed critical packet structure mismatches between frontend and backend**
  - LoginRequestPacket: Fixed string encoding from 16-bit to 8-bit length prefixes to match backend fixed arrays
  - RequestSpawnPacket: Added missing Requester and Target GUID fields (was empty 4-byte packet vs expected 36 bytes)
  - ChatPacket: Added missing channel field and fixed string encoding to match backend structure
  - Implemented robust packet synchronization with validity checking for PacketId values
  - Added enhanced debugging for packet corruption with hex dumps and recovery logging
- **Code quality improvements**
  - Eliminated ~400 lines of inline packet manipulation and handler registration code
  - Perfect separation of concerns - each packet type handles its own logic
  - Easier debugging with structured packet data and centralized packet logic
  - More reliable network communication with proper packet boundary handling
  - NetworkManager now under 300 lines vs previous 600+ lines
  - Fixed packet stream desynchronization issues causing "Invalid packet length: 0" errors

## [Unreleased] - 2025-01-19

### Updated PixiJS Client to Use GUIDs for Entity IDs - 2025-01-19
- **Migrated from uint32 to GUID-based entity identification system**
  - Updated all packet handlers to read/write 16-byte GUIDs instead of 4-byte uint32s
  - Modified PacketHandler class to include GUID conversion utilities (guidToBytes/bytesToGuid)
  - Updated NetworkManager to handle GUID-based entity IDs in all packet processing
  - Changed Entity and World classes to use string IDs throughout the ECS

### Fixed Frontend Packet Handlers for GUID Support - 2025-01-20
- **Resolved packet parsing errors with server's NTT-based GUID system**
  - Fixed AssociateIdPacket handler to match server's fixed 17-byte name format
  - Updated ChatPacket handler to read GUID user IDs and fixed 256-byte message format
  - Added bounds checking and debug logging to prevent buffer overrun errors
  - Resolved "Cannot read string of length 368 at offset 22" error
  - Fixed "Player 0" display issue in chat by correctly parsing GUID user IDs
  - Updated all Component base class and derived components to accept string entityId
  - Modified System base class utility methods to work with string entity IDs
  - Updated GameScreen to track localPlayerId as string instead of number
  - Fixed all TypeScript type definitions to reflect GUID usage
  - Debug entities now use string suffix (`_debug`) instead of numeric offset
  - Packet size adjustments:
    - PlayerMovementPacket: 22 → 34 bytes
    - LoginResponsePacket: 34 → 46 bytes
    - StatusPacket: 17 → 29 bytes
    - MovementPacket: 32 → 44 bytes
  - Compatible with server-side NTT entity system using System.Guid

## [Unreleased] - 2025-01-14

### TypeScript ECS Compilation Fixes - 2025-01-14
- **Fixed major TypeScript compilation errors in ECS system**
  - Fixed Component `changedTick` visibility issue by making it protected instead of private
  - Fixed System componentTypes type mismatch by using flexible `any[]` type
  - Fixed LifetimeSystem to properly extend System base class with required methods
  - Fixed EnergyComponent property access issues (`current` → `availableCharge`)
  - Fixed NetworkManager entity.add() method call (changed to entity.set())
  - Reduced TypeScript errors from ~200 down to ~55 (mostly unused variable warnings)
  - ECS system now compiles properly with major architecture issues resolved

### Client Simulation Persistence Fix - 2025-01-14
- **Fixed client physics simulation stopping when server disconnects**
  - **Root cause**: PhysicsSystem was skipping non-locally-controlled entities when connected
  - Added connection state tracking to PhysicsSystem and NarrowPhaseSystem
  - When disconnected, physics simulation runs for ALL entities, not just locally controlled ones
  - Added monitorConnectionState() method to detect connection changes in real-time
  - GameScreen.blur() no longer stops the game loop to maintain client-side prediction
  - Focus/blur events only affect rendering, not physics simulation
  - Players can now continue experiencing physics and collisions while disconnected from server
  - Fixed issue where opening dev tools or tab switches would freeze the game
  - Client-side entities continue moving and colliding independently after server disconnect

### SIMD Vectorized Collision Resolver - 2025-01-14
- **Added SIMD vectorization for high-performance collision detection and resolution**
  - Implemented AVX/SSE vectorized collision detection using System.Runtime.Intrinsics
  - Added batch processing for up to 8 collision pairs simultaneously with AVX256
  - Created vectorized impulse calculations for better CPU utilization
  - Added DetectCircleCollisionSIMD method using Vector128 operations
  - Implemented BatchDetectCollisionsAVX for parallel collision broad-phase
  - Added CalculateImpulseSIMD for vectorized relative velocity calculations
  - All vectorized methods include scalar fallbacks for non-SIMD hardware
  - Collision detection now processes 4 collision pairs per AVX instruction
  - Maintains identical physics behavior while leveraging modern CPU SIMD units
  - Expected 2-4x performance improvement on AVX-capable processors

### Collision Stability Optimization - 2025-01-14
- **Dramatically improved collision stability for stacked and resting objects**
  - Increased solver iterations from 2 to 4 sub-steps for better convergence
  - Added Baumgarte stabilization (factor 0.2) to resolve deep penetrations smoothly
  - Implemented velocity damping (0.99) and angular damping (0.98) to prevent jitter
  - Limited maximum penetration correction per frame to 0.05 units
  - Reduced response coefficient to 0.8 to prevent overcorrection bouncing
  - Added sleep detection for very slow objects (< 0.1 velocity threshold)
  - Applied extra damping (0.9 factor) to nearly stopped objects
  - Both server and client now use identical stability parameters
  - Fixes erratic behavior when multiple objects stack or rest against each other
  - Prevents "explosion" effect from deep penetrations in dense object clusters

### Complete Angular Momentum Collision System - 2025-01-14
- **Restored and properly implemented angular momentum calculations in collision resolution**
  - Server: Added proper impulse-based angular velocity changes for all collision types
  - Server: Implemented separate methods for circle-circle and complex shape collisions
  - Server: Circle collisions now calculate contact points and apply torque correctly
  - Server: Complex shape collisions use full contact point analysis with angular momentum
  - Client: Mirrored server's angular momentum calculations exactly for consistency
  - Client: Added applyCircleImpulse method with proper rotational physics
  - Both systems calculate perpendicular vectors and angular contributions to contact velocity
  - Both systems apply angular impulses using cross product (r × F) for torque calculation
  - Angular momentum properly conserved through (r.x * impulse.y - r.y * impulse.x) * invInertia
  - Maintains sub-stepping (2 passes) for stability while including full angular physics

### Complete Particle System Implementation - 2025-01-14
- **Implemented comprehensive client-side particle system matching server behavior**
  - Created LifetimeComponent with lifetime tracking and fade-out transparency
  - Implemented LifetimeSystem with progressive alpha transparency (fades from 1.0 to 0.3)
  - Updated EntityType enum to match server's flags-based system (Static=1, Passive=2, Pickable=4, etc.)
  - Added heuristic entity type detection in NetworkManager (small circles → particles)
  - Enhanced PhysicsSystem to destroy particles when hitting floor/ceiling boundaries (like server)
  - Verified NarrowPhaseSystem excludes particles from entity-entity collisions
  - Confirmed projectiles pass through particles (no collision detection)
  - Added despawn packet handling for player pickup functionality (StatusType.Alive=0)
  - Particle behavior now identical to server: spawn → bounce → fade → pickup/expire/boundary death
  - Integrated LifetimeSystem into game loop with proper system dependencies

## [Unreleased] - 2025-01-13

### Complete Collision System Implementation - 2025-01-14
- **Implemented full client-side collision prediction system**
  - Created CollisionComponent matching server architecture
  - Implemented complete Collisions helper class with identical physics math to server
  - Added NarrowPhaseSystem with full impulse-based collision resolution
  - Integrated collision system into game loop (runs after physics, before prediction)
  - Added vertex transformation and collision detection for all shape types
  - Client now predicts collisions accurately using same physics as server
  - Mass-based position separation and proper impulse calculations implemented
  - Updated CLAUDE.md with PixiJS client architecture documentation

### Major Prediction System Simplification - 2025-01-14
- **Dramatically simplified client-server synchronization by directly applying server state**
  - Removed complex reconciliation, input replay, and collision detection systems from PredictionSystem
  - NetworkSystem now directly applies server position/velocity to local player physics
  - Eliminated fighting between prediction and server by trusting server authority completely
  - Prediction system now only maintains state for smooth interpolation between updates
  - Removed collision grace periods, velocity reconciliation thresholds, and position correction logic
  - Much simpler, more reliable approach that prevents all client-server conflicts
  - Reduced code complexity by ~500 lines while improving stability

### Velocity Reconciliation Improvements - 2025-01-14
- **Fixed velocity drift and jumping issues in prediction system**
  - Lowered velocity reconciliation threshold from 50 to 5 units/sec for more accurate corrections
  - Made velocity lerping more aggressive (increased from 0.3 to 0.6 factor) to prevent drift
  - Added gentle correction (0.1 factor) for small velocity differences (>2 units/sec)
  - Implemented deceleration drift detection to prevent jumping after high-speed movement
  - Added aggressive velocity reset when server has stopped but client is still moving
  - Fixed drag factor inconsistency by hardcoding to match server (0.02)
  - Added special handling for low-speed scenarios to eliminate residual velocity differences

### Camera Following Softness Update - 2025-01-14
- **Made camera following much softer and smoother**
  - Reduced follow strength from 6 to 2.5 for gentler tracking
  - Implemented exponential smoothing using `1 - Math.exp()` for buttery-smooth transitions
  - Reduced velocity prediction strength from 0.15 to 0.08 to minimize overshooting
  - Added small dead zone (0.01) to prevent micro-jittering
  - Camera now glides smoothly to target instead of jumping

### Camera Following Improvements - 2025-01-13
- **Enhanced camera smoothness and following behavior**
  - Improved interpolation algorithm with consistent smooth following
  - Added velocity-based prediction for more natural camera movement
  - Reduced minimum movement thresholds to prevent stuttering
  - Camera now smoothly leads the player's movement direction
  - Eliminated jerky camera movements during target switches
  - Better initialization when first setting follow target

### Performance Display Addition - 2025-01-13
- **Added always-visible performance display in top-left corner**
  - Shows FPS counter updated in real-time
  - Displays current client tick from TickSynchronizer
  - Shows server tick when available from NetworkComponent
  - Displays frame delta time in milliseconds
  - Compact overlay design with semi-transparent background
  - Fixed code corruption in StatsPanel.ts and GameScreen.ts from earlier modifications

### PixiJS Client Codebase Cleanup - 2025-01-13
- **Cleaned up pixiejsClient directory structure**
  - Removed temporary and cache files from node_modules (~backup files and .log files)
  - Removed generated manifest.json from src directory (now in .gitignore)
  - Removed .assetpack cache directory
  - Updated eslint.config.mjs to ignore additional directories (node_modules, public/assets, .assetpack)
  - Enhanced .gitignore with build output patterns (dist/, build/)
  - Final project size reduced to 3.1MB (excluding 408MB node_modules)
- **Project organization improvements**
  - Better separation of source code (3.1MB) from dependencies (408MB)
  - Cleaner git tracking with proper ignore patterns
  - Improved development workflow with updated linting configuration

### Movement Packet Velocity Enhancement - 2025-01-13

#### **Added Velocity to Movement Packets**
- **Server**: Enhanced `MovementPacket` struct to include `Vector2 Velocity` field
- **Server**: Updated `NetSyncSystem` to send actual physics velocity in movement packets
- **Client**: Modified movement packet handler to read velocity directly from server
- **Client**: Replaced estimated velocity calculations with accurate server velocity data
- **Benefits**:
  - More accurate velocity synchronization between client and server
  - Improved prediction system accuracy with real velocity data
  - Better reconciliation logic with proper velocity comparison
- **Packet Structure**: `MovementPacket` now includes Position, Velocity, and Rotation from server physics

### BaseResources Dynamic API & Client-Server Size Sync - 2025-01-13

#### **Replaced Static BaseResources.json with Dynamic API**
- **Server**: Added `/api/baseresources` endpoint that serves BaseResources data dynamically
- **Client**: Updated to fetch base resources from API instead of static JSON file
- **Removed**: BaseResources.json file - no longer needed
- **Benefits**: Resources can now be updated without recompiling or redeploying static assets

#### **Fixed Player Size/Shape Discrepancy**
- **Size Issue**: Client was rendering players at size 12 while server simulated them at size 45
  - **Root Cause**: Client ignored server-sent dimensions and used BaseResources template sizes instead
  - **Fix**: Client now uses actual server-sent width/height for entity size
- **Shape Issue**: Server was creating players as boxes (4 sides) instead of circles
  - **Root Cause**: LoginRequest handler used `CreateBoxBody` instead of `CreateCircleBody`
  - **Fix**: Changed server to create players as circles with radius 22.5 (equivalent to diameter 45)
- **Result**: Visual representation now matches physics simulation exactly

#### **Improved Resource Handling**
- Server now serves BaseResources with camelCase property names
- Client handles both camelCase and PascalCase for backward compatibility
- More robust fallbacks when resource data is unavailable

### Server & Client Tick Rate Increase to 60hz - 2025-01-13

#### **Increased Simulation Rate from 30hz to 60hz**
- **Server**: Updated `Game.TargetTps` from 30 to 60 for higher fidelity simulation
- **Client TickSynchronizer**: Updated to synchronize with 60hz server tick rate
- **PredictionSystem**: Physics replay now uses 1/60 deltaTime for accurate server matching
- **InputBuffer**: Increased capacity to 4 seconds (240 ticks) to handle higher tick rate
- **History Management**: Input cleanup now preserves 2 seconds at 60hz (120 ticks)

#### **Benefits**
- Smoother, more responsive gameplay with higher update frequency
- More accurate physics simulation and collision detection
- Better client-side prediction with finer time resolution
- Reduced visual stuttering during fast movements

### Client-Side Prediction & TickCounter Integration - 2025-01-13

#### **Implemented Proper Tick-Based Client-Side Prediction**
- **TickSynchronizer**: New singleton service to synchronize client ticks with server using LoginResponse TickCounter
  - Accounts for network latency using existing ping system
  - Provides accurate server tick estimates for prediction
- **InputSystem Updates**: Replaced client-generated sequence numbers with server-synchronized tick numbers
  - Input snapshots now use actual server ticks for proper timeline matching
  - Movement packets sent with correct server tick instead of timestamp
- **NetworkComponent Enhancement**: Added tick-based synchronization data
  - Removed client-side sequence generation
  - Added configurable reconciliation thresholds
  - Better tracking of last confirmed server state
- **Hybrid Reconciliation**: Smart reconciliation system that adapts to discrepancy size
  - Small errors (≤15px): Smooth lerp correction
  - Large errors (>15px): Full input replay with server state reset
  - Fallback to lerp if input history unavailable
- **NetworkSystem Integration**: Proper server tick tracking from movement packets

#### **Technical Details**
- Reconciliation now uses exact server tick matching instead of approximations
- Input replay works with synchronized tick timeline for accurate prediction
- Maintains 2 seconds of input history for reconciliation (60 ticks at 30 TPS)
- Leverages existing RTT measurement from ping system for latency compensation

### Player Rotation Fix - Complete Engine System Implementation - 2025-01-13

#### **Fixed Player Rotation/Turning**
- **Root cause identified**: Client was incorrectly applying torque directly to physics instead of using server's Engine→Physics pipeline
- **EngineSystem implementation**: Created complete EngineSystem matching server exactly
  - Applied server logic from `EngineSystem.cs` line-by-line: power calculations, energy constraints, propulsion forces
  - Implemented exact angular velocity application: `physics.angularVelocity = engine.rotation * 3` (matching server line 32)
  - Added RCS-based drag calculation: `physics.drag = engine.rcs ? 0.01 : 0.001` matching server values
  - Added throttle-based propulsion with server-exact forward vector calculation
- **InputSystem refactor**: Updated to match server `InputSystem.cs` exactly
  - Removed direct physics manipulation and torque application
  - Added `configureEngine()` method matching server logic (lines 109-129 of server InputSystem.cs)
  - Proper rotation input mapping: left/right keys set `engine.rotation = -1/1/0` instead of direct angular velocity
  - Added gradual throttle control: boost sets throttle to 1, thrust/invThrust gradually modify throttle
  - Added RCS flag handling matching server behavior
- **Component integration**: All spawned entities now receive EngineComponent by default
- **System execution order**: Added EngineSystem with proper priority (98) between InputSystem (100) and PredictionSystem (95)
- **Server parity achieved**: Client now processes input → engine → physics in identical order to server

### PixiJS Client Complete Server Alignment - 2025-01-13

#### **Critical ECS Architecture Overhaul for Client-Side Prediction**
- **Server-identical PhysicsComponent**: Completely rewritten to match server struct exactly
  - Changed from `velocity` to `linearVelocity` matching server field names
  - Changed from `rotation` to `rotationRadians` matching server naming
  - Added `ShapeType` enum with Circle, Triangle, Box matching server values
  - Added server-exact properties: `density`, `inertia`, `width`, `height`, `color`, `sides`
  - Added server-exact computed properties: `radius`, `mass`, `invMass`, `invInertia`, `forward`
  - Added server-exact update flags: `transformUpdateRequired`, `aabbUpdateRequired`, `changedTick`
  - Replaced generic mass calculation with proper area-based density system
  - Added proper inertia calculation for different shapes matching server physics

#### **Server-Identical Physics Simulation**
- **PhysicsSystem complete rewrite**: Now matches server physics step-by-step
  - Applied exact server constants: SPEED_LIMIT (400), MAP_SIZE (1500x100000), GRAVITY_FORCE (9.81)
  - Implemented server-exact physics order: integrate velocity → integrate position → drag → gravity → speed limit → boundaries → thresholds
  - Changed drag calculation from `Math.pow(1-drag, deltaTime)` to simple `1-drag` matching server
  - Added gravity system matching server (applied when Y > 98500)
  - Added speed limiting at 400 units/second exactly matching server
  - Added boundary collision with elastic response matching server behavior
  - Added velocity thresholds (0.1) matching server anti-jitter system
  - Fixed impulse application to use `invMass` instead of division

#### **Complete Component Library Matching Server**
- **InputComponent**: New component matching server `InputComponent` struct exactly
  - Added `PlayerInput` enum with all server input flags (W, A, S, D, Shift, R, Fire, Q, E, Space)
  - Added `movementAxis`, `mouseDir`, `buttonStates`, `didBoostLastFrame` matching server fields
  - Added server-exact input flag manipulation methods

- **EngineComponent**: New component matching server `EngineComponent` struct exactly
  - Added server-exact fields: `powerUse`, `throttle`, `maxPropulsion`, `rcs`, `rotation`, `changedTick`
  - Implemented server power calculation: `powerUse = maxPropulsion * 2`

- **WeaponComponent**: New component matching server `WeaponComponent` struct exactly
  - Added server-exact fields: `fire`, `frequency`, `lastShot`, `bulletDamage`, `bulletCount`, `bulletSize`, `bulletSpeed`, `powerUse`, `direction`
  - Added server-exact power cost calculation: `powerUse * bulletCount * bulletSpeed / 100`

- **EnergyComponent**: Completely rewritten to match server `EnergyComponent` struct
  - Renamed from generic energy to server-exact: `dischargeRateAcc`, `dischargeRate`, `chargeRate`, `availableCharge`, `batteryCapacity`
  - Added server-exact power management methods matching server behavior

- **ShieldComponent**: Completely rewritten to match server `ShieldComponent` struct
  - Added server-exact fields: `powerOn`, `lastPowerOn`, `charge`, `maxCharge`, `powerUse`, `powerUseRecharge`
  - Added server-exact shield mechanics: `minRadius`, `targetRadius`, `rechargeDelayMs`, `lastDamageTime`
  - Implemented server-exact shield radius calculation: `max(minRadius, targetRadius * chargePercent)`
  - Added server-exact damage absorption and recharge delay system

- **AABBComponent**: New component matching server collision detection
  - Added server-exact AABB structure for spatial partitioning
  - Added potential collision tracking for broad-phase collision detection

#### **Entity Type System Alignment**
- **EntityType enum**: Updated to match server exactly
  - Changed from string-based to numeric enum matching server values
  - Updated to: Static(0), Passive(1), Pickable(2), Projectile(3), Npc(4), Player(5)

#### **Architecture Preparation for Client-Side Prediction**
- All components now have identical field names, types, and behaviors to server
- All physics calculations now produce identical results to server simulation
- All constants and thresholds now match server values exactly
- Component change tracking (`changedTick`) implemented for delta compression
- Foundation ready for tick-based client prediction and server reconciliation

#### **Code Quality and ECS Pattern Compliance**
- Removed all ECS pattern violations and non-server-matching abstractions
- All components now pure data structures matching server structs
- All systems now follow server execution patterns exactly
- Eliminated client-specific physics modifications that would break prediction

## [Previous] - 2025-01-13

### Major ECS Cleanup & Client Prediction
- **Removed legacy gameEntities system**: Complete elimination of old Map-based entity storage
- **Pure ECS architecture**: All entity data now flows through proper Component system
- **Client-side prediction**: InputSystem now simulates player movement locally for zero-lag input
- **Component expansion**: Added ShieldComponent, BatteryComponent, InventoryComponent, LevelComponent
- **InputComponent**: Renamed from ControlComponent to match server implementation exactly
- **NetworkManager refactor**: Now properly updates ECS components instead of global entity store
- **Status packet integration**: All server status updates flow into appropriate ECS components
- **Local prediction physics**: Added thrust, friction, RCS, and speed limiting matching server values

### Comprehensive Player Stats Display
- **EntityStatsDisplay**: New reusable UI component for displaying comprehensive entity statistics
- **Feature parity with WebClient**: Complete stats display matching the original WebClient/UiRenderer.js functionality
- **ECS-integrated data extraction**: Automatically extracts data from BatteryComponent, HealthComponent, EnergyComponent, ShieldComponent, and InventoryComponent
- **Bottom left positioning**: Player stats displayed in bottom left corner (configurable for other positions)
- **Storage statistics**: Shows capacity, usage, and breakdown of triangles, squares, pentagons inventory
- **Battery/Power system**: Displays capacity, charge, charge/discharge rates, and power consumption by engine/weapon/shield
- **Engine metrics**: Real-time throttle percentage and power draw based on input state
- **Shield information**: Power usage, current/max charge, and shield radius
- **Health/Energy status**: Current and maximum values when available from ECS components
- **Placeholder handling**: Shows "N/A" for unavailable data instead of breaking the display
- **Dynamic updates**: Real-time data refresh each frame with input state integration
- **Responsive design**: Automatically repositions on canvas resize events
- **Toggle visibility**: F11 key to show/hide player stats display
- **Reusable architecture**: Can be instantiated for multiple entities (player, targets, etc.)
- **ECS pattern compliance**: Maintains separation of concerns with pure data extraction from components

### Fixed
- Fixed camera stuck at origin (0,0) by properly connecting NetworkManager to ClientGame's setLocalPlayer method
- Fixed player spawning at wrong position by using server-provided coordinates in ECS entity creation
- Fixed incorrect player shape rendering by adding RenderComponent to store shape data from server
- Fixed gun barrel pointing to mouse instead of forward direction
- Fixed input system to be server-authoritative (removed local physics modifications)
- Modified World.createEntity to support specific entity IDs for server-client entity mapping

## [Previous] - 2025-09-13

### ECS Architecture Refactor - Major Overhaul
- **Static World Pattern**: Converted World to static singleton for simplified access
- **Proper ECS Separation**: Components now pure data, Systems contain all logic
- **Component Standardization**: All components extend base Component class with change tracking
- **System Dependency Management**: Systems can declare dependencies and execution priorities
- **Query-based Entity Processing**: Systems use World.queryEntitiesWithComponents() for efficient filtering
- **Circular Dependency Resolution**: Eliminated require() calls with global references
- **Type Safety**: Full TypeScript compilation without errors
- **Camera Integration**: Fixed camera system to follow ECS entities properly
- **Network-ECS Bridge**: NetworkManager creates and updates ECS entities from server data

### Architecture Changes
- **World.ts**: Now fully static with getInstance() pattern, manages system execution order
- **Component.ts**: Enhanced with serialization, change tracking, and debug utilities
- **System.ts**: Abstract base with query utilities and World access methods
- **Entity.ts**: Streamlined with proper component lifecycle management
- **PhysicsComponent**: Pure data with utility methods for common operations
- **NetworkComponent**: Proper Component extension with prediction state
- **All Systems**: Refactored to use World queries instead of manual entity lists

### Added
- Complete ECS-based frontend implementation in TypeScript
- Client-side prediction system for zero-latency player movement
- Entity interpolation for smooth remote entity rendering  
- Snapshot buffer system for network state management
- Prediction buffer with input reconciliation
- Binary packet protocol matching server implementation
- NetworkManager handling WebSocket communication
- InputManager for keyboard, mouse and touch controls
- ClientRenderSystem with interpolated rendering
- Separation of fixed timestep physics and variable timestep rendering
- Debug overlay showing FPS, ping, network stats
- HTML5 game interface with login form and UI elements

### Architecture
- Frontend now mirrors server's ECS architecture
- Components: Physics, Network, Health, Energy, etc.
- Systems: Input, Physics, Network, Render
- Client-side prediction with server reconciliation
- Interpolation delay buffer for smooth remote entities
- Fixed 60Hz physics tick with variable rendering

### Technical Details
- TypeScript with Vite build system
- Canvas-based rendering with camera system
- Support for thousands of entities
- Optimized rendering with viewport culling
- Network packets use binary format for efficiency
- Input sent at 60Hz to server
- Snapshots buffered with 100ms interpolation delay

## [2025-09-13] - Full Input System and UI Overlay

### Added
- Complete input system supporting all server keybinds:
  - **W/↑** - Thrust (forward movement)
  - **S/↓** - Reverse thrust (backward movement)
  - **A/←** - Turn left
  - **D/→** - Turn right
  - **Shift** - Boost (increased thrust)
  - **R** - RCS (reaction control system) - **Toggle**
  - **Left Click** - Fire weapons
  - **Q/E** - Drop items from inventory
  - **Space** - Shield activation
- Real-time input state display overlay showing:
  - All keybind states with visual indicators
  - Current mouse position and movement vector
  - Player health and energy bars
  - Engine throttle percentage
  - Power consumption in kW
  - RCS activation status
- InputOverlay component with:
  - Toggle visibility with F12 key
  - Dynamic sizing based on content
  - Health/Energy bars with percentage display
  - Engine throttle and power draw indicators
- EngineComponent for tracking thrust and power states
- Enhanced packet protocol supporting all PlayerInput flags
- Automatic thrust decay when releasing W key

### Technical Details
- InputManager maps all keyboard/mouse inputs to server PlayerInput enum
- PacketHandler creates 22-byte movement packets with full input flags
- UI overlay positioned in top-right corner with semi-transparent background
- Real-time statistics from actual player entity when available
- F12 toggles input display, F3 toggles debug info
- Support for touch controls on mobile devices
- RCS toggle mechanism - press R to activate/deactivate, persists until toggled again

### Status Bar System
- **Player Status Bars**: Fixed position UI bars (top-left) showing health, energy, and shield
- **Target Status Bars**: Dynamic bars that follow entities around the screen
  - Positioned below each entity
  - Automatically track entity movement in real-time
  - Only visible for entities within 80% of view distance
  - Smaller scale (70%) to avoid screen clutter
  - Show health, energy, and shield for nearby targets
- **StatusBarManager**: Manages multiple bar instances efficiently
- **Camera Integration**: Bars correctly transform with camera zoom and position

### Status Bar Fixes
- **Fixed shield bar flickering**: Now uses actual entity data instead of random values
- **Entity-specific data**: Each entity's bars show their own health/energy/shield values
- **Proper StatusType mapping**: Updated to use correct server enum values (Health=1, Energy=11, Shield=20, etc.)
- **Real-time updates**: Bars update immediately when server sends Status packets
- **Throttle integration**: Engine throttle shows actual server values when available
- **Conditional rendering**: Only show bars for stats that have actual data

### Critical Packet Parsing Fix
- **Fixed StatusPacket structure**: Now matches server exactly
  - Header (4 bytes)
  - UniqueId (4 bytes)
  - Value (8 bytes as `double`, not `float`)
  - Type (1 byte as `StatusType`)
- **Correct field order**: Value comes before Type, not after
- **Proper data types**: Using `getFloat64()` for double values
- **Eliminated corrupted values**: No more `3.8685626227668134e+25` garbage data
- **Complete StatusType mapping**: Added all 23 status types from server enum
- **Enhanced logging**: Added detailed packet debugging and status change tracking

### Status Bar Values Fix
- **Dynamic min/max values**: Status bars now use actual server values instead of hardcoded defaults
- **Player bars**: Use real `health/maxHealth`, `energy/maxEnergy`, `shieldCharge/shieldMaxCharge`
- **Target bars**: Only show bars when both current AND max values are available from server
- **Proper scaling**: Health bars now show 1000 max instead of 100, Energy shows 100,000 max
- **Real-time updates**: Bar proportions update correctly as server sends new min/max values
- **Conditional rendering**: Bars only appear when server has sent the required data

### Shield Visual Rendering
- **Shield rendering system**: Added visual shield effects around entities with ShieldComponent
- **Dynamic shield appearance**: Shield color and opacity change based on charge level (0.1 → 0.2)
  - High charge (>50%): Blue to cyan gradient with high visibility
  - Low charge (<50%): Red to yellow gradient indicating critical state
- **Radial gradient effect**: Glowing shield bubble with fade-out edges
- **Dashed border**: Active shields show animated dashed border for clear visual feedback
- **Charge-based transparency**: Shield opacity scales with charge percentage (minimum 15% visibility)
- **Integrated with RenderSystem**: Shields render after entity sprites, using entity position and physics data