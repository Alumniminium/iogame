using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct BodyDamageComponent(NTT EntityId, float damage = 1)
{
    public readonly NTT EntityId = EntityId;
    public readonly float Damage = damage;


}