using System.Security.Policy;
using iogame.ECS;

namespace iogame.Simulation.Components
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