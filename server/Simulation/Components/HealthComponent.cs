using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public float Health;
        public readonly int MaxHealth;
        public readonly float PassiveHealPerSec;
        public uint ChangedTick;

        public HealthComponent(float health, int maxHealth, float healthRegFactor)
        {
            Health = health;
            MaxHealth = maxHealth;
            PassiveHealPerSec = healthRegFactor;
            ChangedTick = 0;
        }
    }
    [Component]
    public struct ShieldComponent
    {
        public float Charge;
        public readonly int MaxCharge;
        public readonly float PowerUse;
        public uint ChangedTick;

        public ShieldComponent(float charge, int maxCharge, float powerUse)
        {
            Charge = charge;
            MaxCharge = maxCharge;
            PowerUse = powerUse;
            ChangedTick = 0;
        }
    }
}