using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct ViewportComponent(int entityId, float viewDistance)
{
    public readonly int EntityId = entityId;
    public readonly List<PixelEntity> EntitiesVisible = new();
    public readonly List<PixelEntity> EntitiesVisibleLast = new();
    public RectangleF Viewport = new RectangleF(0, 0, viewDistance, viewDistance);

    public override int GetHashCode() => EntityId;
}