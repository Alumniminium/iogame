using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct SpeedComponent
    {
        public uint Speed;

        public SpeedComponent(uint speed)
        {
            Speed=speed;
        }
    }
}