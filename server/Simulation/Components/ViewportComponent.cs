using System.Drawing;
using server.ECS;
using server.Simulation.Entities;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public List<ShapeEntity> EntitiesVisible;
        public List<ShapeEntity> EntitiesVisibleLastSync;
        public RefList<PixelEntity> AddedEntities;
        public RefList<PixelEntity> RemovedEntities;
        public readonly int ViewDistance;
        public RectangleF Viewport;


        public ViewportComponent(int viewDistance)
        {
            ViewDistance = viewDistance;
            EntitiesVisible = new (50);
            EntitiesVisibleLastSync = new (40);
            AddedEntities = new(40);
            RemovedEntities = new (40);
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
        }
    }
}