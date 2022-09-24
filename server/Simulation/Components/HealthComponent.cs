using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public float Health;
        public readonly int MaxHealth;
        public uint ChangedTick;

        public HealthComponent(float health, int maxHealth)
        {
            Health = health;
            MaxHealth = maxHealth;
            ChangedTick = Game.CurrentTick;
        }
    }
}