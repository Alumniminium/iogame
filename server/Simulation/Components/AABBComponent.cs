using System.Collections.Generic;
using System.Drawing;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct AABBComponent
    {
        public readonly int EntityId;
        public RectangleF AABB;
        public readonly List<PixelEntity> PotentialCollisions;

        public AABBComponent(int entityId, RectangleF aabb)
        {
            EntityId = entityId;
            AABB = aabb;
            PotentialCollisions = new ();
        }
        public override int GetHashCode() => EntityId;
    }
}