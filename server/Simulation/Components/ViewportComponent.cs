using System;
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
        public int ViewDistance;
        public RectangleF Viewport;
    }
}