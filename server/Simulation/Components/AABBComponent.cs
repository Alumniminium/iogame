using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct AABBComponent(int entityId, RectangleF aabb)
{
    public readonly int EntityId = entityId;
    public RectangleF AABB = aabb;
    public readonly List<PixelEntity> PotentialCollisions = new();

    public override int GetHashCode() => EntityId;
}