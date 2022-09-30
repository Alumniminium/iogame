using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct CollisionComponent
    {
        public readonly int EntityId;
        public readonly EntityType EntityTypes;
        public readonly PixelEntity A;
        public readonly PixelEntity B;
        public readonly Vector2 Impulse;

        public CollisionComponent(PixelEntity a, PixelEntity b, Vector2 impulse)
        {
            EntityId = a.Id;
            A = a;
            B = b;
            Impulse = impulse;
            EntityTypes = a.Type | b.Type;
        }
        public override int GetHashCode() => EntityId;
    }
}