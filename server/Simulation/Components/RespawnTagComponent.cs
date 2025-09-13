using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct RespawnTagComponent(int entityId, int expPenalty, int respawnTimeDelaySeconds)
{
    public readonly int EntityId = entityId;
    public readonly int ExpPenalty = expPenalty;
    public readonly long RespawnTimeTick = Game.CurrentTick + Game.TargetTps * respawnTimeDelaySeconds;

    public override int GetHashCode() => EntityId;
}