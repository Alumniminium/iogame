using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct NameTagComponent(NTT EntityId, string name)
{
    public readonly NTT EntityId = EntityId;
    public readonly string Name = name;


}