using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            if(float.IsNaN(dmg.Damage) || float.IsInfinity(dmg.Damage) || float.IsNaN(hlt.Health) || float.IsInfinity(hlt.Health))
                return;
                
            hlt.Health -= Math.Clamp(dmg.Damage, 0, hlt.Health);
            hlt.ChangedTick = Game.CurrentTick;
            ntt.Remove<DamageComponent>();

            if (hlt.Health > 0)
                return;

            var dtc = new DeathTagComponent(dmg.AttackerId);
            ntt.Add(ref dtc);
        }
    }
}