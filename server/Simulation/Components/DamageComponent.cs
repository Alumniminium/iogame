using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct DamageComponent
    {
        public readonly int AttackerId;
        public float Damage;

        public DamageComponent(int attackerId, float damage)
        {
            AttackerId = attackerId;
            Damage = damage;
        }
    }
}