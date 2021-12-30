using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: 1) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();
                ref readonly var dmg = ref entity.Get<DamageComponent>();

                hlt.Health -= dmg.Damage;
                entity.Remove<DamageComponent>();
            }
        }
    }
}