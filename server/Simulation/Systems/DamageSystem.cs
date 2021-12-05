using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System") { }
        public override void Update(float dt, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
                // for (int i = 0; i < Entities.Count; i++)
                // {
                //     var entity = Entities[i];
                //     ref var hlt = ref entity.Get<HealthComponent>();
                //     ref readonly var dmg = ref entity.Get<DamageComponent>();

                //     hlt.Health -= dmg.Damage;
                //     entity.Remove<DamageComponent>();
                // }
            }
        }
    }
}