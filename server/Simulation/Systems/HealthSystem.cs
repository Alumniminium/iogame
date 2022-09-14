using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent c1)
        {
            var lastHealth = c1.Health;
            c1.Health += c1.PassiveHealPerSec * deltaTime;

            if (c1.Health > c1.MaxHealth)
                c1.Health = c1.MaxHealth;

            if (lastHealth != c1.Health)
                c1.ChangedTick = Game.CurrentTick;
        }
    }
}