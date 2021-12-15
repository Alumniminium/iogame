using System.Drawing;
using server.ECS;
using server.Simulation.Entities;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly List<ShapeEntity> EntitiesVisible;
        public readonly List<ShapeEntity> EntitiesVisibleLastSync;
        public readonly int ViewDistance;
        public RectangleF Viewport;


        public ViewportComponent(int viewDistance)
        {
            ViewDistance = viewDistance;
            EntitiesVisible = new List<ShapeEntity>();
            EntitiesVisibleLastSync = new List<ShapeEntity>();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
        }
    }
}