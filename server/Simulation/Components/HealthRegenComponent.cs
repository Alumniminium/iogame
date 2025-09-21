using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct HealthRegenComponent(NTT EntityId, float healthRegFactor)
{
    public readonly NTT EntityId = EntityId;
    public readonly float PassiveHealPerSec = healthRegFactor;


}