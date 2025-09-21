# AGENTS.md - Development Guidelines for iogame

## Build & Test Commands
- **Build server**: `dotnet build server/server.csproj`
- **Run server**: `cd server && dotnet run`
- **Build solution**: `dotnet build iogame.sln`
- **Debug server**: Use VSCode "SERVER" launch config
- **Web client**: Serve `WebClient/index.html` (no build needed)
- **No test framework configured** - run manual testing

## Code Style Guidelines

### C# (.NET 9)
- **Namespaces**: File-scoped (`namespace MyNamespace;`)
- **Braces**: No braces for single-line if statements
- **Nullable**: Disabled for compatibility
- **Unsafe**: Enabled for performance-critical paths
- **Imports**: Group system imports first, then project imports
- **ECS Pattern**: Components as data-only structs, Systems inherit NttSystem<T>
- **Naming**: PascalCase for types/methods, camelCase for locals
- **Error Handling**: Use exceptions for exceptional cases, validate inputs

### JavaScript (ES6)
- **Modules**: Use ES6 import/export syntax
- **Classes**: Modern class syntax with constructor/methods
- **Naming**: camelCase for variables/functions, PascalCase for classes
- **Async**: Use async/await for asynchronous operations

### Architecture Patterns
- **ECS**: Components in `server/Simulation/Components/`, Systems in `server/Simulation/Systems/`
- **Networking**: Binary packets in `Shared/`, handlers in `server/Simulation/Net/`
- **Entity Creation**: Use `SpawnManager.cs` for consistent initialization
- **Performance**: Object pooling, array-of-structures, spatial partitioning

### Development Workflow
- Add components with `[Component]` attribute
- Systems auto-register via inheritance from `NttSystem<T>`
- Game loop order: Spawn → Input → Physics → Collision → Gameplay → Network
- Viewport culling for client-side performance