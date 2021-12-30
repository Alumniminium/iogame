using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", threads: 1) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var lif = ref entity.Get<LifeTimeComponent>();

                lif.LifeTimeSeconds -= dt;

                if (lif.LifeTimeSeconds <= 0)
                    PixelWorld.Destroy(in entity);
            }
        }
    }
}