using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BulletComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Owner;

        public BulletComponent(in PixelEntity owner)
        {
            EntityId = owner.Id;
            Owner = owner;
        }
        public override int GetHashCode() => EntityId;
    }
}