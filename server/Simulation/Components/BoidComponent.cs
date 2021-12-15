using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BoidComponent
    {
        public readonly byte Flock;

        public BoidComponent(byte flock)
        {
            Flock = flock;
        }
    }
}