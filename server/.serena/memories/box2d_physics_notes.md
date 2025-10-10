# Box2D Physics Integration

## CRITICAL: Box2D Coordinate System
**NEVER GET THIS WRONG!**

### Coordinate Axes
- **Positive Y = DOWN** (gravity is +9.81 Y)
- **Negative Y = UP**
- **Positive X = RIGHT**
- **Negative X = LEFT**

### Rotation
- **0° = pointing RIGHT** (positive X direction)
- **+90° = pointing DOWN** (positive Y direction)
- **-90° = pointing UP** (negative Y direction)
- **+180° = pointing LEFT** (negative X direction)

### Force Application
- Apply forces in the direction you want the object to move
- **For upward thrust**: use **negative Y force** to counteract positive Y gravity
- **Standard forward direction**: `new Vector2(MathF.Cos(rotation), MathF.Sin(rotation))`
- **To point UP**: spawn with **-90° rotation**, which gives forward = (0, -1) = UP

## Mass vs Density

Box2D uses density, not mass directly:
- **Actual mass = density × area**
- **For desired mass**: `density = desiredMass / (width × height)`

Example:
```csharp
float desiredMass = 100f;
float width = 2f;
float height = 2f;
float density = desiredMass / (width * height); // 25
```

## Physics Systems

### Box2DEngineSystem
- Processes engine thrust and RCS (reaction control system)
- Applies forces to Box2D bodies
- Uses EngineComponent data for thrust values

### ShipPhysicsRebuildSystem
- Rebuilds Box2D bodies when ship parts change
- Handles multi-part ship physics
- Parent-child transform calculations

### Box2DCollisionSystem
- Handles collision detection and response
- Uses Box2D's native collision system
- Integrates with game's damage/pickup systems

## Component: Box2DBodyComponent
- Stores reference to Box2D body
- Links entity to physics simulation
- Managed by physics systems

## Performance Notes
- Box2D runs in Box2DEngineSystem
- Physics simulation at 60 TPS
- Multi-part ships use compound bodies
- Efficient spatial queries via QuadTree