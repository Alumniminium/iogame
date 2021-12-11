using System;
using System.Collections.Generic;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", Environment.ProcessorCount) { }

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
                ref var lif = ref entity.Get<LifeTimeComponent>();

                lif.LifeTimeSeconds -= deltaTime;

                if (lif.LifeTimeSeconds <= 0)
                    PixelWorld.Destroy(entity.EntityId);
            }
        }
    }
}