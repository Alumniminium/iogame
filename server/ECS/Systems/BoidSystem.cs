using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class BoidSystem : PixelSystem<PositionComponent,VelocityComponent,SpeedComponent,BoidComponent>
    {
        public BoidSystem()
        {
            Name = "BoidSystem System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var hlt = ref entity.Get<HealthComponent>();
                var shp = World.GetAttachedShapeEntity(ref entity);

                shp.Viewport.Update();
            }
        }
    }
}