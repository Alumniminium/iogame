using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct SpeedComponent
    {
        public readonly uint Speed;

        public SpeedComponent(uint speed)
        {
            Speed=speed;
        }
    }
}