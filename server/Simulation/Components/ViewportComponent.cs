using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct ViewportComponent(NTT EntityId, float viewDistance)
{
    public readonly NTT EntityId = EntityId;
    public readonly List<NTT> EntitiesVisible = new();
    public readonly List<NTT> EntitiesVisibleLast = new();
    public RectangleF Viewport = new(0, 0, viewDistance, viewDistance);


}