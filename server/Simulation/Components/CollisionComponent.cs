using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct E2EColComponent
    {
        public readonly int WithId;
        public readonly PixelEntity With => PixelWorld.GetEntity(WithId);

        public E2EColComponent(int entityId)
        {
            WithId = entityId;
        }
    }
}