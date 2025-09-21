using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct DeathTagComponent
{
    public readonly NTT Entity;
    public readonly NTT Killer;

    public DeathTagComponent(NTT entityId, NTT killerId)
    {
        Entity = entityId;
        Killer = killerId;
    }
}