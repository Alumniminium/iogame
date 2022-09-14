using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct BodyDamageComponent
    {
        public readonly float Damage;
        public BodyDamageComponent(float damage) => Damage = damage;
    }
}