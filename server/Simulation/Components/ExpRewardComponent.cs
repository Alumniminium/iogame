using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct ExpRewardComponent(int entityId, int experience)
{
    public readonly int EntityId = entityId;
    public readonly int Experience = experience;

    public override int GetHashCode() => EntityId;
}