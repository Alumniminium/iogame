using System.Drawing;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public PixelEntity[] EntitiesVisible;
        public PixelEntity[] EntitiesVisibleLast;
        public RectangleF Viewport;

        public ViewportComponent(float viewDistance)
        {
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            EntitiesVisible = System.Array.Empty<PixelEntity>();
            EntitiesVisibleLast = System.Array.Empty<PixelEntity>();
        }
    }
}