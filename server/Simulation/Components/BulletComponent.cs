using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct BulletComponent(NTT entityId, NTT owner)
{
    public readonly NTT EntityId = entityId;
    public readonly NTT Owner = owner;
}