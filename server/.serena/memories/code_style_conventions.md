# Code Style and Conventions

## C# Conventions

### Namespace Style
- **Use file-scoped namespaces** (required):
```csharp
namespace server.Simulation.Components;
```
NOT:
```csharp
namespace server.Simulation.Components 
{
}
```

### Control Flow Style
- **No braces for single-line if statements**:
```csharp
if (c1.Health == c1.MaxHealth)
    return;
```
NOT:
```csharp
if (c1.Health == c1.MaxHealth)
{
    return;
}
```

### Language Features
- **Unsafe code enabled** for performance-critical paths
- **Primary constructors** used for structs and classes where appropriate:
```csharp
public struct HealthComponent(float health, float maxHealth)
{
    public float Health = health;
    public float MaxHealth = maxHealth;
}
```
- **Nullable reference types disabled** for compatibility
- **.NET 9 preview features** enabled

### Naming Conventions
- **PascalCase** for public fields, methods, properties, classes
- **Components** suffixed with "Component": `HealthComponent`, `EnergyComponent`
- **Systems** suffixed with "System": `HealthSystem`, `DamageSystem`
- **Data-only components**: Components are structs with public fields (no methods)

### Component Structure
Components must follow this pattern:
```csharp
[Component(ComponentType = ComponentType.Health, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HealthComponent(float health, float maxHealth)
{
    /// ChangedTick MUST be first field for raw byte access in ComponentSerializer
    public long ChangedTick = NttWorld.Tick;
    
    public float Health = health;
    public float MaxHealth = maxHealth;
}
```

### System Structure
Systems inherit from `NttSystem<T1, T2, ...>`:
```csharp
public sealed class HealthSystem : NttSystem<HealthComponent, HealthRegenComponent>
{
    public HealthSystem() : base("Health System", threads: 1) { }
    
    public override void Update(in NTT ntt, ref HealthComponent c1, ref HealthRegenComponent c2)
    {
        // System logic here
    }
}
```

### Documentation
- Use XML documentation comments (`///`) for public types and members
- Document system purposes and behaviors clearly