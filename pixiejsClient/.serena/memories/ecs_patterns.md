# ECS Implementation Patterns

## Core ECS Components

### World (`src/app/ecs/core/World.ts`)
- Central ECS coordinator
- Manages entity lifecycle
- Executes systems in defined order
- Provides entity queries

### Entity (`src/app/ecs/core/Entity.ts`)
- Lightweight container with unique ID
- Component storage via Map
- add/remove/get component methods

### Component (Base class pattern)
- Data-only classes extending base Component
- No logic, just properties
- Examples: Position, Velocity, Health, Shield, Transform

### System (`src/app/ecs/core/System.ts`)
- Extend base System class
- Define component requirements in constructor
- Override `update(entity, deltaTime)` method
- World automatically provides matching entities

## Adding New Components

1. Create component class in `src/app/ecs/components/`
```typescript
export class MyComponent extends Component {
  value: number = 0;
}
```

2. Add to ComponentType enum in `src/app/enums/ComponentIds.ts`
```typescript
export enum ComponentType {
  // ... existing
  MyComponent = XX,
}
```

3. If networked, add deserialization in `ComponentStatePacket.ts`

## Adding New Systems

1. Create system class in `src/app/ecs/systems/`
```typescript
export class MySystem extends System {
  constructor() {
    super([MyComponent, OtherComponent]);
  }

  update(entity: Entity, deltaTime: number): void {
    const my = entity.get(MyComponent)!;
    const other = entity.get(OtherComponent)!;
    // Update logic here
  }
}
```

2. Register in GameScreen initialization
3. Systems execute in registration order

## Component Sync from Server

1. Server sends ComponentStatePacket with component type + data
2. NetworkSystem deserializes based on ComponentType enum
3. Creates or updates component on client entity
4. RenderSystem uses updated data for visual display

## Query Patterns
- World provides entity filtering
- Systems automatically get matching entities
- Manual queries available via World methods
