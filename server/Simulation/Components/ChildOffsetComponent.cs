using System.Numerics;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct ChildOffsetComponent(NTT EntityId, int parentId, Vector2 offset, float rotationOffset)
{
        public readonly NTT EntityId = EntityId;
        public readonly int ParentId = parentId;
        public readonly Vector2 Offset = offset;
        internal readonly float Rotation = rotationOffset;
}