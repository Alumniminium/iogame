using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct CollisionComponent
    {
        public readonly PixelEntity A;
        public readonly PixelEntity B;

        public CollisionComponent(in PixelEntity a, in PixelEntity b)
        {
            A=a;
            B=b;
        }
    }
}