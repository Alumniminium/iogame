# Changelog

## [Unreleased] - 2025-01-13

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