using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct DamageComponent
    {
        public readonly float Damage;

        public DamageComponent(float damage)
        {
            Damage = damage;
        }
    }
}