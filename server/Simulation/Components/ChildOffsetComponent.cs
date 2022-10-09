using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ChildOffsetComponent
    {
        public readonly int EntityId;
        public readonly int ParentId;
        public readonly Vector2 Offset;
        internal readonly float Rotation;

        public ChildOffsetComponent(int entityId, int parentId, Vector2 offset, float rotationOffset)
        {
            EntityId = entityId;
            ParentId = parentId;
            Offset = offset;
            Rotation = rotationOffset;
        }
    }
}