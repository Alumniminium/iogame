using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BulletComponent
    {
        public readonly PixelEntity Owner;

        public BulletComponent(PixelEntity owner) => Owner = owner;
    }
}