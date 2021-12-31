using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BulletComponent
    {
        public readonly PixelEntity Owner;

        public BulletComponent(in PixelEntity owner) => Owner = owner;
    }
}