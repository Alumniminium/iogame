using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct DeathTagComponent(int entityId, int killerId)
{
    public readonly int EntityId = entityId;
    public readonly int KillerId = killerId;

    public override int GetHashCode() => EntityId;
}