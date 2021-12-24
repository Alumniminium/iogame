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
        public readonly List<ShapeEntity> AddedEntities;
        public readonly List<ShapeEntity> RemovedEntities;
        public readonly List<ShapeEntity> ChangedEntities;
        public readonly int ViewDistance;
        public RectangleF Viewport;


        public ViewportComponent(int viewDistance)
        {
            ViewDistance = viewDistance;
            EntitiesVisible = new List<ShapeEntity>();
            EntitiesVisibleLastSync = new List<ShapeEntity>();
            AddedEntities = new List<ShapeEntity>();
            RemovedEntities = new List<ShapeEntity>();
            ChangedEntities = new List<ShapeEntity>();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
        }
    }
}