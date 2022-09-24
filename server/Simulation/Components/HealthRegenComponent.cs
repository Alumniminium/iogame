using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct HealthRegenComponent
    {
        public readonly float PassiveHealPerSec;

        public HealthRegenComponent(float healthRegFactor) => PassiveHealPerSec = healthRegFactor;
    }
}