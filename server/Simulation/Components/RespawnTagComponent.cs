using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct RespawnTagComponent
    {
        public readonly int EntityId;
        public readonly int ExpPenalty;
        public readonly long RespawnTimeTick;

        public RespawnTagComponent(int entityId, int expPenalty, int respawnTimeDelaySeconds)
        {
            EntityId = entityId;
            RespawnTimeTick = Game.CurrentTick + Game.TargetTps * respawnTimeDelaySeconds;
            ExpPenalty = expPenalty;
        }
        public override int GetHashCode() => EntityId;
    }
}