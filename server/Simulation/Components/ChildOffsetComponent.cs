using System.Numerics;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct ChildOffsetComponent(int entityId, int parentId, Vector2 offset, float rotationOffset)
{
        public readonly int EntityId = entityId;
        public readonly int ParentId = parentId;
        public readonly Vector2 Offset = offset;
        internal readonly float Rotation = rotationOffset;
}