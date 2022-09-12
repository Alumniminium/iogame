using System.Collections.Generic;
using System.Drawing;
using server.ECS;
using server.Simulation.Entities;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public List<ShapeEntity> EntitiesVisible;
        public List<ShapeEntity> EntitiesVisibleLast;
        public readonly int ViewDistance;
        public RectangleF Viewport;


        public ViewportComponent(int viewDistance)
        {
            ViewDistance = viewDistance;
            EntitiesVisible = new(50);
            EntitiesVisibleLast = new(40);
            Viewport = new Rectangle(0, 0, viewDistance, viewDistance);
        }
    }
}