using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Damage, NetworkSync = true)]
public struct DamageComponent(NTT attackerId, float damage)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly NTT Attacker = attackerId;
    public float Damage = damage;
}