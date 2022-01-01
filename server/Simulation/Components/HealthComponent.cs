using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public float Health;
        public readonly int MaxHealth;
        public readonly float PassiveHealPerSec;

        public HealthComponent(float health, int maxHealth, float healthRegFactor)
        {
            Health = health;
            MaxHealth=maxHealth;
            PassiveHealPerSec = healthRegFactor;
        }
    }
}