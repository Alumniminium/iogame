using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BodyDamageComponent
    {
        public readonly int EntityId;
        public readonly float Damage;
        public BodyDamageComponent(int entityId, float damage = 1)
        {
            EntityId = entityId;
            Damage = damage;
        }

        public override int GetHashCode() => EntityId;
    }
}