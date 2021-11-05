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
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();

                if (hlt.Health <= 0)
                {
                    World.Destroy(entity.EntityId);
                    return;
                }

                if (hlt.Health < hlt.MaxHealth)
                {
                    var healthAdd = 0;//1 * deltaTime;
                    if (hlt.Health + healthAdd > hlt.MaxHealth)
                        hlt.Health = hlt.MaxHealth;
                    else
                        hlt.Health += healthAdd;
                }

                if(!entity.Has<DamageComponent>())
                    continue;

                ref var dmg = ref entity.Get<DamageComponent>();
                hlt.Health -= dmg.Damage;
            }
        }
    }
}