using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.HealthRegen, NetworkSync = true)]
public struct HealthRegenComponent(float healthRegFactor)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly float PassiveHealPerSec = healthRegFactor;
}