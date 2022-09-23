using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct RespawnTagComponent
    {
        public readonly int ExpPenalty;
        public readonly long RespawnTimeTick;

        public RespawnTagComponent(int expPenalty, int respawnTimeDelaySeconds)
        {
            RespawnTimeTick = Game.CurrentTick + Game.TargetTps * respawnTimeDelaySeconds;
            ExpPenalty = expPenalty;
        }
    }
}