using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public readonly int EntityId;
        public float Health;
        public readonly int MaxHealth;
        public uint ChangedTick;

        public HealthComponent(int entityId, float health, int maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
            ChangedTick = Game.CurrentTick;
        }
        public override int GetHashCode() => EntityId;
    }
}