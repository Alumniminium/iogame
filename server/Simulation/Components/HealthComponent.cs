using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public float Health;
        public int MaxHealth;
        public float HealthRegenFactor;

        public HealthComponent(float health, int maxHealth, float healthRegFactor)
        {
            Health = health;
            MaxHealth=maxHealth;
            HealthRegenFactor = healthRegFactor;
        }
    }
}