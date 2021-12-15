using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System",1) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();
                ref readonly var dmg = ref entity.Get<DamageComponent>();

                hlt.Health -= dmg.Damage;
                entity.Remove<DamageComponent>();
            }
        }
    }
}