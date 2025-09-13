using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DamageComponent(int entityId, int attackerId, float damage)
{
    public readonly int EntityId = entityId;
    public readonly int AttackerId = attackerId;
    public float Damage = damage;

    public override int GetHashCode() => EntityId;
}