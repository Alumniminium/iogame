using System.Collections.Generic;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System") { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
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