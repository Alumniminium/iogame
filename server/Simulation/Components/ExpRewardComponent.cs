using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct ExpRewardComponent(NTT EntityId, int experience)
{
    public readonly NTT EntityId = EntityId;
    public readonly int Experience = experience;


}