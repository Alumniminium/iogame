using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct DamageComponent
    {
        public readonly int AttackerId;
        public readonly float Damage;

        public DamageComponent(int attackerId, float damage)
        {
            AttackerId = attackerId;
            Damage = damage;
        }
    }
}