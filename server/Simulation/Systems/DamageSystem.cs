using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent c1, ref DamageComponent c2)
        {
            c1.Health -= c2.Damage;
            c1.ChangedTick = Game.CurrentTick;
            ntt.Remove<DamageComponent>();

            if (c1.Health > 0)
                return;

            var dtc = new DeathTagComponent(c2.AttackerId);
            ntt.Add(ref dtc);
        }
    }
}