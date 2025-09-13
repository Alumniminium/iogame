using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct HealthComponent(int entityId, float health, int maxHealth)
{
        public readonly int EntityId = entityId;
        public float Health = health;
        public readonly int MaxHealth = maxHealth;
        public uint ChangedTick = Game.CurrentTick;

    public override int GetHashCode() => EntityId;
    }