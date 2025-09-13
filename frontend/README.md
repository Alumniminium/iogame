# ECS Game Frontend

A TypeScript frontend implementation that mirrors the backend ECS (Entity Component System) architecture.

## Architecture

This frontend replicates the backend ECS pattern with:

- **Entity**: Objects that represent game entities
- **Components**: Data-only classes that define entity properties
- **Systems**: Logic that operates on entities with specific component combinations
- **World**: Manages the ECS lifecycle and system orchestration

## Features

- **Type-Safe ECS**: Full TypeScript support with proper typing
- **Reactive UI**: HTML/CSS UI that updates based on component changes
- **Canvas Rendering**: Game objects rendered on HTML5 Canvas
- **Modular Architecture**: Easy to extend with new components and systems
- **Performance Optimized**: Only updates UI when components actually change

## Project Structure

```
frontend/
├── src/
│   ├── ecs/
│   │   ├── core/           # Core ECS classes
│   │   │   ├── Entity.ts
│   │   │   ├── Component.ts
│   │   │   ├── System.ts
│   │   │   ├── World.ts
│   │   │   └── types.ts
│   │   ├── components/     # Component implementations
│   │   └── systems/        # System implementations
│   ├── game/
│   │   ├── Game.ts         # Main game orchestrator
│   │   └── entities/       # Entity classes
│   ├── ui/                 # UI management
│   ├── rendering/          # Canvas rendering
│   └── utils/              # Utility classes
├── public/
│   ├── index.html          # Main HTML file
│   └── styles.css          # CSS styles
├── package.json
├── tsconfig.json
├── vite.config.ts
└── .eslintrc.js
```

## Getting Started

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Start development server:**
   ```bash
   npm run dev
   ```

3. **Open browser:**
   Navigate to `http://localhost:3000`

## Controls

- **WASD** or **Arrow Keys**: Move player
- Watch the UI bars update in real-time as you play

## Key Components

### Core ECS Classes

- **Entity**: Represents game objects with component management
- **Component**: Data containers (Health, Physics, Level, Energy)
- **System**: Logic that processes entities with specific components
- **World**: Manages entity lifecycle and system updates

### Systems

- **HealthSystem**: Handles health regeneration
- **PhysicsSystem**: Updates position and velocity
- **UiSystem**: Updates HTML UI based on component changes
- **RenderSystem**: Renders entities to canvas

### Components

- **HealthComponent**: Health and max health values
- **PhysicsComponent**: Position, velocity, size, mass
- **LevelComponent**: Level and experience tracking
- **EnergyComponent**: Battery charge and consumption

## Adding New Features

### New Component
```typescript
export class NewComponent extends Component {
  value: number;

  constructor(entityId: number, value: number) {
    super(entityId);
    this.value = value;
  }
}
```

### New System
```typescript
export class NewSystem extends System {
  readonly componentTypes = [NewComponent];

  protected updateEntity(entity: Entity, deltaTime: number): void {
    const component = entity.getComponent(NewComponent)!;
    // Your logic here
  }
}
```

### Register System
```typescript
// In Game.ts
this.world.addSystem(new NewSystem());
```

## Development

- **Linting**: `npm run lint`
- **Build**: `npm run build`
- **Preview**: `npm run preview`

## Consistency with Backend

This frontend ECS mirrors the backend architecture:
- Same component structure and naming
- Similar system patterns
- Entity lifecycle management
- Change notification system

This ensures consistency between client and server game logic.