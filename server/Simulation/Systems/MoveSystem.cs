using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class MoveSystem : PixelSystem<PositionComponent, PhysicsComponent>
    {
        public const int SpeedLimit = 3750;
        public MoveSystem() : base("Move System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();

                if(entity.Has<EngineComponent>())
                {
                    ref readonly var eng = ref entity.Get<EngineComponent>();
                    phy.Acceleration = eng.Propulsion * dt;
                }

                phy.Velocity += phy.Acceleration;
                phy.Velocity *= 1f - phy.Drag;
                
                phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);
                
                if (phy.Velocity.Length() < 1 && phy.Acceleration == Vector2.Zero)
                    phy.Velocity = Vector2.Zero;
                    
                pos.LastPosition = pos.Position;
                var newPosition = pos.Position + phy.Velocity * dt;
                pos.Position = newPosition;

                if (pos.Position == pos.LastPosition)
                    continue;

                pos.Rotation = (float)Math.Atan2(newPosition.Y - pos.Position.Y, newPosition.X - pos.Position.X);
            }
        }
    }
}