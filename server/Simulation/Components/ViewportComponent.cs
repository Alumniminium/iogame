using System.Collections.Generic;
using System.Drawing;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Viewport, NetworkSync = false)]
public struct ViewportComponent(float viewDistance)
{
    public readonly List<NTT> EntitiesVisible = [];
    public readonly List<NTT> EntitiesVisibleLast = [];
    public RectangleF Viewport = new(0, 0, viewDistance, viewDistance);


}