using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct HealthRegenComponent(int entityId, float healthRegFactor)
{
        public readonly int EntityId = entityId;
        public readonly float PassiveHealPerSec = healthRegFactor;

    public override int GetHashCode() => EntityId;
    }