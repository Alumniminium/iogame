using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct DeathTagComponent(NTT EntityId, NTT killerId)
{
    public readonly NTT Entity = EntityId;
    public readonly NTT Killer = killerId;


}