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

        public readonly Vector2 MoveB;
        public readonly Vector2 MoveA;
        public readonly Vector2 AccelA;
        public readonly Vector2 AccelB;

        public CollisionComponent(PixelEntity a, PixelEntity b, Vector2 moveA = default, Vector2 moveB = default, Vector2 accelA = default, Vector2 accelB = default)
        {
            A = a;
            B = b;
            EntityTypes = a.Type | b.Type;
            MoveA = moveA;
            MoveB = moveB;
            AccelA = accelA;
            AccelB = accelB;
        }
    }
}