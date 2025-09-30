using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.ExpReward, NetworkSync = true)]
public struct ExpRewardComponent(int experience)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly int Experience = experience;
}