using System.Security.Policy;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct DamageComponent
    {
        public int AttackerEntityId;
        public float Damage;

        public DamageComponent(int attackerId, float damage)
        {
            AttackerEntityId = attackerId;
            Damage = damage;
        }
    }
}