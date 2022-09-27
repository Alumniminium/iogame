using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct ExpRewardComponent
    {
        public readonly int EntityId;
        public readonly int Experience;

        public ExpRewardComponent(int entityId, int experience)
        {
            EntityId = entityId;
            Experience = experience;
        }
        public override int GetHashCode() => EntityId;
    }
}