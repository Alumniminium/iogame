using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct ExpRewardComponent
    {
        public readonly int Experience;

        public ExpRewardComponent(int experience) => Experience = experience;
    }
}