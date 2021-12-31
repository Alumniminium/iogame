using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", threads: 12) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt)
        {
            hlt.Health += hlt.PassiveHealPerSec * deltaTime;

            if (hlt.Health > hlt.MaxHealth)
                hlt.Health = hlt.MaxHealth;

            if (Math.Abs(hlt.LastHealth - hlt.Health) < 0.5f)
                return;

            hlt.LastHealth = hlt.Health;

            if (hlt.Health <= 0)
                PixelWorld.Destroy(in ntt);
        }
    }
}