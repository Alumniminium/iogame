using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct BulletComponent(in PixelEntity owner)
{
    public readonly int EntityId = owner.Id;
    public readonly PixelEntity Owner = owner;

    public override int GetHashCode() => EntityId;
}