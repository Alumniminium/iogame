using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct DeathTagComponent
    {
        public readonly int EntityId;
        public readonly int KillerId;
        public DeathTagComponent(int entityId, int killerId)
        {
            EntityId = entityId;
            KillerId = killerId;
        }
        public override int GetHashCode() => EntityId;
    }
}