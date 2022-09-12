using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public List<PixelEntity> EntitiesVisible;
        public List<PixelEntity> EntitiesVisibleLast;
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