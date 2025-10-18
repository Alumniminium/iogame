# ShipBuilder Redesign - ECS-Driven Architecture

## Problem Statement

The current ShipBuilder uses a **type-based approach** that conflicts with the ECS philosophy:
```typescript
// WRONG: Hardcoded types
interface ShipPartData {
  type: "hull" | "shield" | "engine";  // ❌ Not ECS-driven
  shape: "triangle" | "square";
  rotation: number;
  attachedComponents?: AttachedComponent[];
}
```

This approach:
- Limits flexibility - can't create arbitrary combinations
- Doesn't leverage the full ECS component system
- Couples visual representation (shape) with functionality (type)

## Requirements

1. **ECS-Driven**: Each part is just an NTT (entity)
2. **No Types**: No hardcoded "hull/shield/engine" types
3. **Any Component**: All components from ComponentRegistry can be attached
4. **Multiple Components**: Add as many unique components as desired

## Current ECS Architecture

### Entity Structure
- **Ship Part = NTT (Entity)** with components attached
- **ParentChildComponent** links part to player ship
  - Contains: parentId, gridX, gridY, shape, rotation
  - This is the ONLY required component for a ship part

### Component System
**Available Components** (from ComponentIds.ts):
- Physics (1) - Box2D body
- Gravity (2) - Gravity source
- Health (3) - Hit points
- Shield (4) - Energy shield
- Energy (5) - Power storage
- Engine (6) - Propulsion
- Weapon (30) - Offensive capability
- HealthRegen (18) - Healing
- BodyDamage (19) - Contact damage
- Effect (43) - Visual effects
- And 35+ more...

**Component Registry**:
- All components auto-register via `@component(componentType)` decorator
- ComponentStatePacket handles serialization for ALL component types
- Server validates and processes components

### Network Flow
1. Client sends ComponentStatePacket for each component
2. Server receives and validates
3. Server creates NTT with components
4. Server syncs back to all clients
5. Clients render based on components

## New Design - Pure ECS Approach

### Core Principle
**A ship part is ONLY defined by:**
1. **Shape** (visual representation): Circle, Box, Triangle
2. **Rotation** (orientation): 0°, 90°, 180°, 270°
3. **Grid Position** (from ParentChildComponent)
4. **Attached Components** (functionality): ANY components from registry

### Data Structures

```typescript
// Pure ECS approach - no "types"
interface ShipPartData {
  shape: ShapeType;  // Visual only
  rotation: number;  // 0-3 (90° increments)
  components: ComponentAttachment[];  // Functionality
}

type ShapeType = "circle" | "box" | "triangle";

interface ComponentAttachment {
  componentType: ComponentTypeId;
  parameters: ComponentParameters;
}

// Component-specific parameters
type ComponentParameters = 
  | EngineParameters
  | ShieldParameters
  | WeaponParameters
  | HealthParameters
  | EnergyParameters
  // ... etc for all component types
```

### UI Architecture

#### 1. Shape Selection
```
┌─────────────────────────┐
│   Select Shape:         │
│  ○ Circle               │
│  ■ Box      (selected)  │
│  △ Triangle             │
└─────────────────────────┘
```

#### 2. Component Selection
```
┌─────────────────────────────────────┐
│  Available Components               │
│  ─────────────────────────          │
│  🔧 Engine                    [Add] │
│  🛡️  Shield                    [Add] │
│  🔫 Weapon                    [Add] │
│  ❤️  Health                    [Add] │
│  ⚡ Energy                    [Add] │
│  🔄 Health Regen             [Add] │
│  💥 Body Damage              [Add] │
│  ... (35+ more components)          │
└─────────────────────────────────────┘
```

#### 3. Attached Components List
```
┌─────────────────────────────────────┐
│  Attached Components                │
│  ────────────────────               │
│  🔧 Engine (50N thrust)      [Edit] [Remove] │
│  🛡️  Shield (100 charge)     [Edit] [Remove] │
│  ❤️  Health (100 HP)         [Edit] [Remove] │
└─────────────────────────────────────┘
```

#### 4. Component Configuration Dialog
When adding a component, show parameter inputs:
```
┌─────────────────────────────┐
│  Configure Engine           │
│  ─────────────────          │
│  Max Thrust: [50] Newtons   │
│  RCS Enabled: ☑️            │
│                             │
│  [Cancel]  [Add Component]  │
└─────────────────────────────┘
```

### Complete Build Flow

1. **Enter Build Mode** (press B)
   - BuildGrid appears
   - Shape selector appears

2. **Select Shape**
   - User picks: Circle, Box, or Triangle
   - Shape selector updates selection

3. **Add Components**
   - Component picker appears
   - User browses component list
   - Click "Add" → Component config dialog
   - Enter parameters → Component added to list
   - Repeat for more components

4. **Place Part**
   - Ghost preview shows on grid (shape + visual indicators for components)
   - Click to place
   - All ComponentStatePackets sent to server:
     - ParentChildComponent (required - has shape, rotation, grid position)
     - Each attached component packet

5. **Server Processing**
   - Server validates components
   - Creates NTT with all components
   - Syncs back to clients

### Implementation Plan

#### Phase 1: Data Structure Refactor
1. Remove `type` field from ShipPartData
2. Add `ShapeType = "circle" | "box" | "triangle"`
3. Change `attachedComponents` to use ComponentTypeId

#### Phase 2: Component Parameter System
1. Create component parameter interfaces for each component type
2. Build parameter validation
3. Create component config dialogs

#### Phase 3: UI Components
1. **ShapeSelector** - Add circle option
2. **ComponentPicker** - New component to browse ComponentRegistry
3. **ComponentConfigDialog** - Parameter input for each component
4. **AttachedComponentsList** - Show/edit/remove attached components
5. **BuildDialog** - Main dialog orchestrating the flow

#### Phase 4: Network Integration
1. Update ShipPartManager to send arbitrary components
2. Remove type-based logic
3. Send ComponentStatePacket for each attached component

#### Phase 5: Rendering
1. Update EntityRenderer to handle components visually
2. Show indicators for attached components (glow, icons, etc.)

## Component Parameters Reference

### Common Component Parameters

**EngineComponent**:
- maxThrust: number (Newtons)
- rcs: boolean (Reaction Control System)

**ShieldComponent**:
- charge: number (current charge)
- maxCharge: number
- radius: number (shield radius)
- minRadius: number
- rechargeRate: number

**WeaponComponent**:
- owner: NTT (entity ID)
- bulletDamage: number
- bulletCount: number
- bulletSize: number
- bulletSpeed: number
- frequency: number (shots per minute)

**HealthComponent**:
- health: number (current HP)
- maxHealth: number

**EnergyComponent**:
- energy: number (current)
- maxEnergy: number
- regenRate: number

**HealthRegenComponent**:
- regenRate: number (HP per second)

**BodyDamageComponent**:
- damage: number (contact damage)

## UI Mockup Structure

### Main Build Dialog
```html
<div class="build-dialog">
  <div class="shape-section">
    <h3>1. Select Shape</h3>
    <shape-selector></shape-selector>
  </div>
  
  <div class="components-section">
    <h3>2. Add Components</h3>
    <div class="split-view">
      <component-picker></component-picker>
      <attached-components-list></attached-components-list>
    </div>
  </div>
  
  <div class="actions">
    <button class="place-btn">Place Part on Grid</button>
    <button class="cancel-btn">Cancel</button>
  </div>
</div>
```

### Component Picker
```html
<div class="component-picker">
  <input type="text" placeholder="Search components..." />
  <div class="component-list">
    <div class="component-item" data-type="6">
      <span class="icon">🔧</span>
      <span class="name">Engine</span>
      <button class="add-btn">Add</button>
    </div>
    <!-- Repeat for all components -->
  </div>
</div>
```

### Benefits of New Design

1. **True ECS Architecture**
   - Parts are pure entities
   - Functionality defined by components
   - Maximum flexibility

2. **Unlimited Combinations**
   - Any component on any shape
   - Multiple components per part
   - No artificial restrictions

3. **Future-Proof**
   - New components automatically available
   - No UI updates needed for new component types
   - Leverages existing ECS infrastructure

4. **User Empowerment**
   - Creative freedom in ship design
   - Discover emergent gameplay
   - Experiment with combinations

## Migration Strategy

### Backward Compatibility
- Old ship parts can be migrated by:
  - "hull" type → Just shape, no extra components
  - "shield" type → Shape + ShieldComponent
  - "engine" type → Shape + EngineComponent

### Testing Plan
1. Create standalone HTML prototypes
2. Test component parameter validation
3. Verify network serialization
4. Load test with many components
5. Performance test rendering

## Open Questions

1. **Component Conflicts**: What if components conflict? (e.g., multiple engines)
   - Solution: Allow only one of each component type per part

2. **Default Values**: Should components have sensible defaults?
   - Solution: Yes, provide suggested values in config dialog

3. **Visual Feedback**: How to show which components are attached?
   - Solution: Icons/glows on the part in the grid

4. **Component Requirements**: Do some components require others?
   - Solution: Show warnings but allow placement (server validates)

5. **Performance**: Many components per part?
   - Solution: Limit to ~10 components per part for UX/performance
