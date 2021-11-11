using System.Runtime.CompilerServices;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem()
        {
            Name = "Lifetime System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var lif = ref entity.Get<LifeTimeComponent>();
                
                lif.LifeTimeSeconds -= deltaTime;

                if (lif.LifeTimeSeconds <= 0)
                    World.Destroy(entity.EntityId);
            }
        }
    }
}