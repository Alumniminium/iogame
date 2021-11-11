using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class InputSystem : PixelSystem<PositionComponent, InputComponent, SpeedComponent, VelocityComponent>
    {
        public InputSystem()
        {
            Name = "Input System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var spd = ref entity.Get<SpeedComponent>();
                ref var pos = ref entity.Get<PositionComponent>();

                var shapeEntity = World.GetAttachedShapeEntity(entity);

                var inputVector = Vector2.Zero;
                if (inp.Left)
                    inputVector.X = -1;
                else if (inp.Right)
                    inputVector.X = 1;

                if (inp.Up)
                    inputVector.Y = -1;
                else if (inp.Down)
                    inputVector.Y = 1;

                inputVector *= spd.Speed;
                inputVector = inputVector.ClampMagnitude(spd.Speed);
                inputVector *= deltaTime;

                shapeEntity.FireDir = (float)Math.Atan2(inp.Y - pos.Position.Y, inp.X - pos.Position.X);

                if(inp.Fire)
                    shapeEntity.Attack();

                vel.Force += inputVector;
            }
        }
    }
}