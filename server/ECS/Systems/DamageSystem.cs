using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem()
        {
            Name = "Damage System";
            PerformanceMetrics.RegisterSystem(Name);
        }
        public override void Update(float dt, List<PixelEntity> Entities)
        {
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