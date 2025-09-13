using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct NameTagComponent(int entityId, string name)
{
    public readonly int EntityId = entityId;
    public readonly string Name = name;

    public override int GetHashCode() => EntityId;
}