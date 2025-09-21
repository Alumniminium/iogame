using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct RespawnTagComponent(NTT EntityId, int expPenalty, int respawnTimeDelaySeconds)
{
    public readonly NTT EntityId = EntityId;
    public readonly int ExpPenalty = expPenalty;
    public readonly long RespawnTimeTick = NttWorld.Tick + Game.TargetTps * respawnTimeDelaySeconds;


}