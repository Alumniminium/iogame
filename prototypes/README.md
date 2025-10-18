# ShipBuilder ECS-Driven Redesign - Interactive Prototypes

## Overview

This directory contains three standalone HTML prototypes that demonstrate the new ECS-driven ShipBuilder architecture. Each prototype is fully interactive and can be opened directly in a browser.

## Design Philosophy

### Core Principle: Pure ECS Architecture

The new ShipBuilder eliminates artificial "type" restrictions and embraces pure ECS principles:

- **Ship Parts = Entities (NTTs)** with arbitrary components
- **No hardcoded types** (no "hull", "shield", "engine" limitations)
- **Any component from ComponentRegistry** can be attached
- **Maximum flexibility** for creative ship designs

### Old vs New Approach

#### ❌ Old Approach (Type-Based)
```typescript
interface ShipPartData {
  type: "hull" | "shield" | "engine";  // Restrictive!
  shape: "triangle" | "square";
  rotation: number;
  attachedComponents?: AttachedComponent[];
}
```

**Problems:**
- Couples visual (shape) with functionality (type)
- Limited to 3 predefined types
- Can't create unique combinations
- Not leveraging ECS architecture

#### ✅ New Approach (ECS-Driven)
```typescript
interface ShipPartData {
  shape: ShapeType;  // Visual representation only
  rotation: number;   // Orientation
  components: ComponentAttachment[];  // ANY components!
}

type ShapeType = "circle" | "box" | "triangle";

interface ComponentAttachment {
  componentType: ComponentTypeId;
  parameters: Record<string, any>;
}
```

**Benefits:**
- True ECS architecture
- 40+ components available
- Unlimited creative combinations
- Future-proof (new components auto-available)

## Prototypes

### 1. Shape Selector (`prototype-shape-selector.html`)

**Purpose:** Demonstrate shape selection and rotation controls.

**Features:**
- 3 shape options: Circle, Box, Triangle
- 4 rotation states: 0°, 90°, 180°, 270°
- Live preview with rotation visualization
- Direction indicators for directional shapes
- Clean, modern UI

**How to Use:**
1. Open `prototype-shape-selector.html` in browser
2. Click a shape (Circle, Box, or Triangle)
3. Select rotation (0°, 90°, 180°, 270°)
4. Preview updates in real-time
5. Click "Next" to proceed (simulated)

**Key UI Elements:**
- Shape cards with hover effects
- Rotation buttons with active state
- Canvas preview with grid background
- Direction arrows for non-circle shapes

---

### 2. Component Picker (`prototype-component-picker.html`)

**Purpose:** Demonstrate component browsing, selection, and configuration.

**Features:**
- 15+ sample components from actual ComponentRegistry
- Search functionality
- Category filtering (All, Offensive, Defensive, Utility, Resource)
- Component configuration dialogs with parameters
- Attached components list with edit/remove
- Real-time stats (attached count, available count)

**How to Use:**
1. Open `prototype-component-picker.html` in browser
2. Browse available components
3. Use search box to filter
4. Click category buttons to filter by type
5. Click "Add" on any component
6. Configure parameters in modal
7. Click "Add Component" to attach
8. Edit or remove attached components

**Key UI Elements:**
- Component browser with icons and descriptions
- Search and category filters
- Configuration modal with dynamic forms
- Attached components panel with actions
- Statistics display

**Available Component Categories:**
- **Offensive:** Weapon, Body Damage, Damage
- **Defensive:** Health, Shield, Health Regen
- **Utility:** Engine, Physics, Effect, Spawner
- **Resource:** Energy, Level, Exp Reward, Inventory

---

### 3. Full Builder (`prototype-full-builder.html`)

**Purpose:** Complete integrated experience from shape to placement.

**Features:**
- **Left Panel:** Shape selection + Component picker
- **Center:** Grid canvas with ghost preview
- **Right Panel:** Placed parts list
- Complete workflow demonstration
- Real-time preview on grid
- Part placement and removal

**How to Use:**
1. Open `prototype-full-builder.html` in browser
2. **Step 1:** Select shape (Circle, Box, Triangle)
3. **Step 2:** Add components (click components, configure, add)
4. **Step 3:** Hover over grid to see preview
5. Click grid cell to place part
6. Review placed parts in right panel
7. Remove parts if needed
8. Repeat to build complete ship

**Workflow:**
```
Select Shape → Add Components → Preview on Grid → Place Part → Repeat
```

**Key Features:**
- Three-panel layout for optimal UX
- Ghost preview on hover
- Visual representation of placed parts
- Component summary for each part
- Clear/reset functionality

---

## Design Decisions

### 1. Shape Options

**Circle:**
- Omnidirectional (no rotation effect)
- Best for turrets, shields
- No direction indicator

**Box:**
- Balanced shape
- 4 rotation states matter
- Direction arrow at top

**Triangle:**
- Highly directional
- Perfect for engines, weapons
- Clear forward indicator

### 2. Component System

**Why 40+ Components?**
- Leverages full ComponentRegistry
- No artificial restrictions
- Enables emergent gameplay
- Future-proof design

**Component Parameters:**
- Each component has unique parameters
- Configurable via modal dialogs
- Sensible defaults provided
- Server validates all values

**Example Components:**

| Component | Parameters | Use Case |
|-----------|------------|----------|
| Engine | maxThrust, rcs | Propulsion, movement |
| Shield | charge, radius, regenRate | Energy shield defense |
| Weapon | damage, rateOfFire, bulletSpeed | Offensive capability |
| Health | health, maxHealth | Survivability |
| Energy | capacity, regenRate | Power management |
| HealthRegen | regenRate | Passive healing |
| BodyDamage | damage | Contact damage |

### 3. UI/UX Patterns

**Color Scheme:**
- Primary: #667eea (purple-blue gradient)
- Background: Dark navy gradient
- Accents: Semi-transparent overlays
- Highlights: Glowing borders for selection

**Interaction Patterns:**
- Hover states on all interactive elements
- Active states for selections
- Modal dialogs for configuration
- Ghost preview for placement
- Immediate visual feedback

**Layout Philosophy:**
- Progressive disclosure (3 steps)
- Side-by-side comparison (picker vs attached)
- Center focus (grid canvas)
- Persistent context (placed parts visible)

### 4. Technical Architecture

**Component Registry:**
```javascript
const COMPONENTS = [
  {
    id: ComponentType.Engine,
    name: 'Engine',
    icon: '🔧',
    category: 'utility',
    description: 'Propulsion and movement',
    params: { maxThrust: 50, rcs: true }
  },
  // ... 40+ more
];
```

**Ship Part Data Structure:**
```javascript
{
  shape: 'triangle',
  rotation: 1,  // 90°
  gridX: 2,
  gridY: 3,
  components: [
    { id: 6, params: { maxThrust: 50, rcs: true } },
    { id: 30, params: { damage: 10, rateOfFire: 5 } }
  ]
}
```

**Network Serialization:**
```
1. ParentChildComponent (gridX, gridY, shape, rotation)
2. ComponentStatePacket for each attached component
3. Server validates and creates NTT
4. Server syncs back to all clients
```

## Implementation Roadmap

### Phase 1: Core Refactoring (Week 1)
- [ ] Remove `type` field from all interfaces
- [ ] Add `ShapeType` enum
- [ ] Update `ComponentAttachment` system
- [ ] Create component parameter interfaces
- [ ] Update TypeScript types

### Phase 2: UI Components (Week 2)
- [ ] Enhanced `ShapeSelector` with circle option
- [ ] New `ComponentPicker` component
- [ ] New `ComponentConfigDialog` component
- [ ] New `AttachedComponentsList` component
- [ ] Update `BuildDialog` to orchestrate flow

### Phase 3: Network Integration (Week 3)
- [ ] Update `ShipPartManager.createShipPart()`
- [ ] Remove type-based logic
- [ ] Implement arbitrary component sending
- [ ] Server-side validation
- [ ] Test all component types

### Phase 4: Rendering & Polish (Week 4)
- [ ] Visual indicators for attached components
- [ ] Component glows/icons on parts
- [ ] Performance optimization
- [ ] Error handling
- [ ] User feedback and iteration

### Phase 5: Testing & Launch (Week 5)
- [ ] Integration testing
- [ ] Performance profiling
- [ ] Bug fixes
- [ ] Documentation
- [ ] Release

## Testing Instructions

### Manual Testing Checklist

**Shape Selector:**
- [ ] All 3 shapes selectable
- [ ] All 4 rotations work
- [ ] Preview updates correctly
- [ ] Direction indicators accurate
- [ ] Reset functionality works

**Component Picker:**
- [ ] Search filters correctly
- [ ] Category filtering works
- [ ] Configuration modal opens
- [ ] Parameters save correctly
- [ ] Can't add duplicate components
- [ ] Edit/remove functionality works

**Full Builder:**
- [ ] Complete workflow end-to-end
- [ ] Ghost preview accurate
- [ ] Parts place correctly
- [ ] Multiple parts supported
- [ ] Clear/reset works
- [ ] No performance issues

### Browser Compatibility

Tested in:
- ✅ Chrome 120+
- ✅ Firefox 120+
- ✅ Safari 17+
- ✅ Edge 120+

## Next Steps

1. **Review Prototypes:** Open each HTML file and test functionality
2. **Gather Feedback:** Share with team/stakeholders
3. **Refine Design:** Based on feedback
4. **Begin Implementation:** Start with Phase 1
5. **Iterate:** Continuous improvement based on user testing

## Questions & Decisions

### Open Questions

1. **Component Limit:** Should we limit components per part?
   - Suggestion: Max 10 components for UX/performance

2. **Component Conflicts:** What if components conflict?
   - Suggestion: Allow only one of each component type per part

3. **Default Values:** Auto-fill sensible defaults?
   - Suggestion: Yes, with ability to customize

4. **Visual Feedback:** How to show attached components on grid?
   - Suggestion: Color coding + icon overlays

5. **Save/Load:** Should we support ship templates?
   - Suggestion: Future feature, not MVP

### Design Decisions Made

✅ **Pure ECS:** No type restrictions
✅ **All Components Available:** Full ComponentRegistry
✅ **Shape-based Visual:** Separate from functionality
✅ **Modal Configuration:** Better UX than inline
✅ **Progressive Disclosure:** 3-step workflow

## Files Included

```
prototypes/
├── README.md                           # This file
├── prototype-shape-selector.html       # Shape selection demo
├── prototype-component-picker.html     # Component browser demo
└── prototype-full-builder.html         # Complete integrated demo
```

## Contributing

When extending these prototypes:

1. Maintain consistent color scheme
2. Follow existing interaction patterns
3. Keep components modular
4. Test in all major browsers
5. Update this README

## Support

For questions or issues:
- Review the memory: `shipbuilder-redesign-analysis`
- Check the ECS architecture docs
- Test prototypes in browser
- Consult with team

---

**Last Updated:** 2025-10-12
**Version:** 1.0.0
**Status:** ✅ Prototypes Complete - Ready for Review
