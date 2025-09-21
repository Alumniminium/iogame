using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DamageComponent(NTT EntityId, NTT attackerId, float damage)
{
    public readonly NTT EntityId = EntityId;
    public readonly NTT Attacker = attackerId;
    public float Damage = damage;


}