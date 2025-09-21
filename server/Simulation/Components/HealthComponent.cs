using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct HealthComponent(NTT EntityId, float health, int maxHealth)
{
    public readonly NTT EntityId = EntityId;
    public float Health = health;
    public readonly int MaxHealth = maxHealth;
    public long ChangedTick = NttWorld.Tick;


}