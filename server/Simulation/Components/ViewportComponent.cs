using System.Drawing;
using server.ECS;
using server.Simulation.Entities;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public ShapeEntity[] EntitiesVisible = Array.Empty<ShapeEntity>();
        public ShapeEntity[] EntitiesVisibleLastSync = Array.Empty<ShapeEntity>();
        public readonly int ViewDistance;
        public RectangleF Viewport;


        public ViewportComponent(int viewDistance)
        {
            ViewDistance = viewDistance;
            EntitiesVisible = Array.Empty<ShapeEntity>();
            EntitiesVisibleLastSync = Array.Empty<ShapeEntity>();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
        }
    }
}