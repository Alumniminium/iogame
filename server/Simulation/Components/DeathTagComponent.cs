using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct DeathTagComponent
    {
        public readonly int KillerId;

        public DeathTagComponent(int killerId)
        {
            KillerId = killerId;
        }
    }
}