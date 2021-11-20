using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class MoveSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public const int SPEED_LIMIT = 1000;
        public MoveSystem()
        {
            Name = "Move System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<Entity> Entities)
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

                // if (vel.Velocity.Magnitude() < 0.1)
                //     vel.Velocity = Vector2.Zero;

                pos.LastPosition = pos.Position;
                pos.Position += vel.Velocity * dt;
                var p2 = pos.Position + vel.Velocity;
                pos.Rotation = (float)Math.Atan2(p2.Y - pos.Position.Y, p2.X - pos.Position.X);
                // pos.Position = Vector2.Clamp(pos.Position, Vector2.Zero, new Vector2(Game.MAP_WIDTH, Game.MAP_HEIGHT));
            }
        }
    }
}