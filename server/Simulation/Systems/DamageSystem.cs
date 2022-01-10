using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            hlt.Health -= dmg.Damage;
            ntt.Remove<DamageComponent>();

            if(hlt.Health > 0)
                return;

            var dtc = new DeathTagComponent(dmg.AttackerId);
            ntt.Add(ref dtc);
        }
    }
}