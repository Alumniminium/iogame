# Changelog

## [Unreleased] - 2025-01-13

### Collision Prediction Fixes - 2025-01-14
- **Fixed major prediction issues after collisions**
  - Added collision detection based on velocity discrepancies (>50 units/sec indicates collision)
  - Disabled input replay when collision suspected to prevent accumulating errors
  - Added collision-aware reconciliation that snaps to server state instead of predicting
  - Implemented early collision detection in main reconciliation loop
  - Added logging for collision detection debugging
  - Client now handles post-collision scenarios much more gracefully

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