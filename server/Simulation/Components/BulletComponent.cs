using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct BulletComponent(NTT owner)
{
    public readonly NTT EntityId = owner;
    public readonly NTT Owner = owner;
}