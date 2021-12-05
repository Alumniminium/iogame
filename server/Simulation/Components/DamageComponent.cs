using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct DamageComponent
    {
        public float Damage;

        public DamageComponent(float damage)
        {
            Damage = damage;
        }
    }
}