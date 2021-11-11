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
        public struct ComponentPair
        {
            public Entity Entity;
            public HealthComponent HealthComponent;
            public DamageComponent DamageComponent;
        }

        public ComponentPair[] Data;
        public bool dirty = true;

        public HealthSystem()
        {
            Name = "Health System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<Entity> Entities)
        {
            if (dirty)
            {
                Data = new ComponentPair[Entities.Count];

                for (int i = 0; i < Entities.Count; i++)
                {
                    var entity = Entities[i];
                    Data[i].Entity = entity;
                    Data[i].HealthComponent = entity.Get<HealthComponent>();

                    if(entity.Has<DamageComponent>())
                        Data[i].DamageComponent = entity.Get<DamageComponent>();
                }
                dirty = false;
            }
            for (int i = 0; i < Data.Length; i++)
            {
                var datum = Data[i];
                ref var entity = ref datum.Entity;
                ref var hlt = ref datum.HealthComponent;
                ref var dmg = ref datum.DamageComponent;
                
                hlt.Health += hlt.HealthRegenFactor * dt;
                hlt.Health -= dmg.Damage;
                                
                if (hlt.Health > hlt.MaxHealth)
                    hlt.Health = hlt.MaxHealth;
                
                if (hlt.Health <= 0)
                    hlt.Health = 0;

                entity.Replace(hlt);
            }
        }
        public override bool MatchesFilter(ref Entity entity)
        {
            var match = base.MatchesFilter(ref entity);
            if (match)
                dirty = true;
            return match;
        }
    }
}