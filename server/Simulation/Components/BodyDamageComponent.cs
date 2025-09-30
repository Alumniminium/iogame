using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.BodyDamage, NetworkSync = true)]
public struct BodyDamageComponent(float damage = 1)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly float Damage = damage;
}