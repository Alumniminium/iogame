using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class DamageSystem : NttSystem<HealthComponent, DamageComponent>
{
    public DamageSystem() : base("Damage System", threads: 1) { }

    public override void Update(in NTT ntt, ref HealthComponent hlt, ref DamageComponent dmg)
    {
        if (ntt.Has<RespawnTagComponent>())
            return;
        if (!NttWorld.EntityExists(dmg.Attacker))
            return;
        var attacker = NttWorld.GetEntity(dmg.Attacker);
        if (ntt.Has<ShieldComponent>())
        {
            ref var shi = ref ntt.Get<ShieldComponent>();
            var dmgAbsorbed = Math.Clamp(dmg.Damage, 0, shi.Charge);
            dmg.Damage -= dmgAbsorbed;
            shi.Charge -= dmgAbsorbed;
            shi.ChangedTick = NttWorld.Tick;
            shi.LastDamageTime = TimeSpan.Zero;
        }
        if (dmg.Damage > 0)
        {
            var rewardableDamage = Math.Min(dmg.Damage, hlt.Health);
            hlt.Health -= Math.Clamp(hlt.Health, 0, dmg.Damage);
            hlt.ChangedTick = NttWorld.Tick;

            if (attacker.Has<LevelComponent>())
            {
                var exp = new ExpRewardComponent(ntt, (int)rewardableDamage);
                attacker.Set(ref exp);
            }

            if (hlt.Health <= 0)
            {
                var dtc = new DeathTagComponent(ntt, dmg.Attacker);
                ntt.Set(ref dtc);
            }
        }
        ntt.Remove<DamageComponent>();
    }
}