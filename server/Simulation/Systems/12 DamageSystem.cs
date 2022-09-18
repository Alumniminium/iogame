using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            if (ntt.Has<ShieldComponent>())
            {
                ref var shi = ref ntt.Get<ShieldComponent>();
                var dmgAbsorbed = Math.Clamp(dmg.Damage, 0, shi.Charge);
                dmg.Damage -= dmgAbsorbed;
                shi.Charge -= dmgAbsorbed;
                shi.ChangedTick = Game.CurrentTick;
            }
            if (dmg.Damage > 0)
            {
                hlt.Health -= dmg.Damage;
                hlt.ChangedTick = Game.CurrentTick;
                
                if (hlt.Health > 0)
                    return;

                var dtc = new DeathTagComponent(dmg.AttackerId);
                ntt.Add(ref dtc);
            }
            ntt.Remove<DamageComponent>();
        }
    }
}