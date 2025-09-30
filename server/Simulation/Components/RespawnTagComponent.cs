using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.RespawnTag, NetworkSync = true)]
public struct RespawnTagComponent(int expPenalty, int respawnTimeDelaySeconds)
{
    public long ChangedTick = NttWorld.Tick;
    public readonly int ExpPenalty = expPenalty;
    public readonly long RespawnTimeTick = NttWorld.Tick + NttWorld.TargetTps * respawnTimeDelaySeconds;
}