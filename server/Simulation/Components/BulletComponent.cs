using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Bullet, NetworkSync = true)]
public struct BulletComponent(NTT owner)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly NTT Owner = owner;
}