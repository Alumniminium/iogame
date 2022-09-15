using server.ECS;
using server.Helpers;
using System.Numerics;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct CollisionComponent
    {
        public readonly EntityType EntityTypes;
        public readonly PixelEntity A;
        public readonly PixelEntity B;

        public CollisionComponent(PixelEntity a, PixelEntity b)
        {
            EntityTypes = a.Type | b.Type;
            A = a;
            B = b;
        }
    }
}