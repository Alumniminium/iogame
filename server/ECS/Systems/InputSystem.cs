using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class InputSystem : PixelSystem<InputComponent, SpeedComponent, VelocityComponent>
    {
        public InputSystem()
        {
            Name = "Input System";
        }

        public override void Update(float deltaTime, List<Entity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var inp = ref entity.Get<InputComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var spd = ref entity.Get<SpeedComponent>();

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

                if (inputVector.Magnitude() == 0)
                    continue;

                inputVector *= spd.Speed;

                inputVector = inputVector.ClampMagnitude(spd.Speed);
                inputVector *= deltaTime;

                vel.Movement += inputVector;
            }
        }
    }
}