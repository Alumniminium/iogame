using System.Numerics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct CollisionComponent
    {
        public readonly EntityType EntityTypes;
        public readonly PixelEntity A;
        public readonly PixelEntity B;
        public readonly Vector2 Impulse;

        public CollisionComponent(PixelEntity a, PixelEntity b, Vector2 impulse)
        {
            A = a;
            B = b;
            Impulse = impulse;
            EntityTypes = a.Type | b.Type;
        }
    }
}