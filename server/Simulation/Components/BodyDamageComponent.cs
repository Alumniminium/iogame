using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct BodyDamageComponent
    {
        public float Damage;
        public BodyDamageComponent(float damage) => Damage = damage;
    }
}