# Task Completion Checklist

When completing a task that involves code changes to the server:

## 1. Build Verification
**ALWAYS run after making changes:**
```bash
dotnet build server/server.csproj
```
Ensure there are no compilation errors or warnings.

## 2. System Registration Check
If you added a new system:
- [ ] System is registered in `Simulation/Game.cs` in the correct order
- [ ] System order matters - verify placement matches game loop requirements
- [ ] Systems are NOT auto-registered - manual registration required

## 3. Component Registration Check
If you added a new component:
- [ ] Component struct created in `Simulation/Components/`
- [ ] `[Component(ComponentType = ComponentType.YourComponent, NetworkSync = true/false)]` attribute added
- [ ] Component type added to `Enums/ComponentIds.cs` enum
- [ ] ChangedTick field is FIRST field if NetworkSync = true
- [ ] If client needs it: TypeScript component added to client codebase
- [ ] If client needs it: ComponentType enum updated in client
- [ ] If client needs it: Deserialization case added in client ComponentStatePacket

## 4. Manual Testing
- [ ] Ask user to run the server and test the changes
- [ ] Do NOT run the server automatically - processes don't exit

## 5. Code Style Verification
- [ ] File-scoped namespaces used (`namespace X;` not `namespace X {}`)
- [ ] Single-line if statements have no braces
- [ ] Components are data-only structs
- [ ] Systems inherit from NttSystem<...>
- [ ] XML documentation added for public types

## Notes
- No automated testing framework configured
- No linting or formatting tools available
- Build verification is the primary automated check