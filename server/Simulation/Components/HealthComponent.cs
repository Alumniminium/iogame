namespace iogame.Simulation.Components
{
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