using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Processes damage application to entities with health, handling shield absorption and death tagging.
/// Awards experience points to attackers for damage dealt.
/// </summary>
public sealed class DamageSystem : NttSystem<HealthComponent, DamageComponent>
{
    public DamageSystem() : base("Damage System", threads: 1) { }

    public override void Update(in NTT ntt, ref HealthComponent hlt, ref DamageComponent dmg)
    {
        if (ntt.Has<RespawnTagComponent>())
            return;
        if (ntt.Has<ShieldComponent>())
        {
            ref var shi = ref ntt.Get<ShieldComponent>();
            var dmgAbsorbed = Math.Clamp(dmg.Damage, 0, shi.Charge);
            dmg.Damage -= dmgAbsorbed;
            shi.Charge -= dmgAbsorbed;
            shi.ChangedTick = NttWorld.Tick;
            shi.LastDamageTimeTicks = NttWorld.Tick;
        }
        if (dmg.Damage > 0)
        {
            var rewardableDamage = Math.Min(dmg.Damage, hlt.Health);
            hlt.Health -= Math.Clamp(hlt.Health, 0, dmg.Damage);
            hlt.ChangedTick = NttWorld.Tick;

            if (hlt.Health <= 0)
            {
                var dtc = new DeathTagComponent(dmg.Attacker);
                ntt.Set(ref dtc);
            }

            if (!NttWorld.EntityExists(dmg.Attacker))
                return;
            var attacker = NttWorld.GetEntity(dmg.Attacker);
            if (attacker.Has<LevelComponent>())
            {
                var exp = new ExpRewardComponent((int)rewardableDamage);
                attacker.Set(ref exp);
            }
        }
        ntt.Remove<DamageComponent>();
    }
}