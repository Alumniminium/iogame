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
            hlt.Health -= dmg.Damage;
            hlt.ChangedTick = Game.CurrentTick;
            ntt.Remove<DamageComponent>();
            Console.WriteLine($"{Game.CurrentTick} - Entity {ntt.Id} took {dmg.Damage} damage, health is now {hlt.Health}");

            if (hlt.Health > 0)
                return;

            var dtc = new DeathTagComponent(dmg.AttackerId);
            ntt.Add(ref dtc);
        }
    }
}