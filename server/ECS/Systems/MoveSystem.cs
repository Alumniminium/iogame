using System.Net.NetworkInformation;
using System.Numerics;
using iogame.ECS;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class MoveSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public const int SPEED_LIMIT = 1000;
        public MoveSystem()  : base(1)
        {
            Name = "Move System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();


                vel.Velocity += vel.Acceleration;
                vel.Velocity = vel.Velocity.ClampMagnitude(SPEED_LIMIT);

                vel.Velocity *= 1f - phy.Drag;

                if (vel.Velocity.Magnitude() < 0.1)
                    vel.Velocity = Vector2.Zero;

                pos.LastPosition = pos.Position;
                var newPosition = pos.Position + vel.Velocity * dt;
                pos.Rotation = (float)Math.Atan2(newPosition.Y - pos.Position.Y, newPosition.X - pos.Position.X);
                pos.Position = newPosition;
            }
        }
    }
}