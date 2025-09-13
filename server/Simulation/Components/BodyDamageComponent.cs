using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct BodyDamageComponent(int entityId, float damage = 1)
{
    public readonly int EntityId = entityId;
    public readonly float Damage = damage;

    public override int GetHashCode() => EntityId;
}