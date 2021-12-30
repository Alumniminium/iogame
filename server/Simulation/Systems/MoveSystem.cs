using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class EngineSystem : PixelSystem<PhysicsComponent, InputComponent, EngineComponent>
    {
        public EngineSystem() : base("Engine System", Environment.ProcessorCount) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var phy = ref entity.Get<PhysicsComponent>();
                ref readonly var eng = ref entity.Get<EngineComponent>();
                ref readonly var inp = ref entity.Get<InputComponent>();

                var propulsion = inp.MovementAxis * eng.MaxPropulsion;
                phy.Acceleration = propulsion * dt;
            }
        }
    }
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 3750;
        public PhysicsSystem() : base("Move System", Environment.ProcessorCount) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                ref var phy = ref entity.Get<PhysicsComponent>();

                phy.Velocity += phy.Acceleration;
                phy.Velocity *= 1f - phy.Drag;
                
                phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);
                
                if (phy.Velocity.Length() < 1 && phy.Acceleration == Vector2.Zero)
                    phy.Velocity = Vector2.Zero;
                    
                phy.LastPosition = phy.Position;
                var newPosition = phy.Position + phy.Velocity * dt;
                phy.Position = newPosition;

                if (phy.Position == phy.LastPosition)
                    continue;

                phy.Rotation = (float)Math.Atan2(newPosition.Y - phy.Position.Y, newPosition.X - phy.Position.X);
            }
        }
    }
}