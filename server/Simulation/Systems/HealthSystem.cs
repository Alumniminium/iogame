using server.ECS;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem() : base("Health System", threads: 1) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();

                hlt.Health += hlt.PassiveHealPerSec * dt;

                if (hlt.Health > hlt.MaxHealth)
                    hlt.Health = hlt.MaxHealth;

                if(Math.Abs(hlt.LastHealth - hlt.Health) < 0.5f)
                    continue;

                hlt.LastHealth = hlt.Health;

                if (hlt.Health <= 0)
                    PixelWorld.Destroy(in entity);
            }
        }
    }
}