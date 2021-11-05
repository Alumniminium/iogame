using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct LifeTimeComponent
    {
        public float LifeTimeSeconds;

        public LifeTimeComponent(TimeSpan timespan) => LifeTimeSeconds = (float)timespan.TotalSeconds;
    }
}