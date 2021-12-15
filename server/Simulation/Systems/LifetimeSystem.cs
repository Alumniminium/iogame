using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", 1) { }

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
                ref var lif = ref entity.Get<LifeTimeComponent>();

                lif.LifeTimeSeconds -= deltaTime;

                if (lif.LifeTimeSeconds <= 0)
                    PixelWorld.Destroy(in entity);
            }
        }
    }
}