using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class InputSystem : PixelSystem<InputComponent, SpeedComponent, VelocityComponent>
    {
        public InputSystem()
        {
            Name = "Input System";
            PerformanceMetrics.RegisterSystem(Name);
        }
        public override void Update(float dt, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var inp = ref entity.Get<InputComponent>();
                ref readonly var spd = ref entity.Get<SpeedComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                var shp = World.GetAttachedShapeEntity(ref entity);

                vel.Acceleration = inp.MovementAxis * spd.Speed * dt;
                shp.FireDir = (float)Math.Atan2(inp.MousePositionWorld.Y, inp.MousePositionWorld.X);

                if(inp.Fire)
                    shp.Attack();

                if(shp is Bullet)
                    entity.Remove<InputComponent>();
            }
        }
    }
}