using System;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public readonly Memory<PixelEntity> EntitiesVisible;
        public readonly Memory<PixelEntity> EntitiesVisibleLast;
        public RectangleF Viewport;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            EntitiesVisible = new PixelEntity[512];
            EntitiesVisibleLast = new PixelEntity[512];
        }
        public override int GetHashCode() => EntityId;
    }
}