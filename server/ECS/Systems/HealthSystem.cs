using System.Runtime.CompilerServices;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class HealthSystem : PixelSystem<HealthComponent>
    {
        public HealthSystem()
        {
            Name = "Health System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();

                if (entity.Has<DamageComponent>())
                {
                    ref readonly var dmg = ref entity.Get<DamageComponent>();
                    hlt.Health -= dmg.Damage;
                }

                hlt.Health += hlt.HealthRegenFactor * dt;

                if (hlt.Health > hlt.MaxHealth)
                    hlt.Health = hlt.MaxHealth;

                if (hlt.Health <= 0)
                {
                    hlt.Health = 0;
                    World.Destroy(entity.EntityId);
                    base.RemoveEntity(ref entity);
                }
            }
        }
    }
}