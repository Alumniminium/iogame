using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", Environment.ProcessorCount) { }
        public override void Update(float deltaTime, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
                ref var lif = ref entity.Get<LifeTimeComponent>();

                lif.LifeTimeSeconds -= deltaTime;

                if (lif.LifeTimeSeconds <= 0)
                    PixelWorld.Destroy(entity.EntityId);
            }
        }
    }
}