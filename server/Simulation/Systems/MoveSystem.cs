using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class MoveSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent>
    {
        public const int SpeedLimit = 750;
        public MoveSystem() : base("Move System", Environment.ProcessorCount/2) { }

        public override void Update(float dt, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
                ref readonly var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();

                vel.Velocity += vel.Acceleration;
                vel.Velocity = vel.Velocity.ClampMagnitude(SpeedLimit);

                vel.Velocity *= 1f - phy.Drag;

                if (vel.Velocity.Length() < 0.1)
                    vel.Velocity = Vector2.Zero;

                pos.LastPosition = pos.Position;
                var newPosition = pos.Position + vel.Velocity * dt;
                pos.Rotation = (float)Math.Atan2(newPosition.Y - pos.Position.Y, newPosition.X - pos.Position.X);
                pos.Position = Vector2.Clamp(newPosition, Vector2.Zero, new Vector2(Game.MapWidth, Game.MapHeight));

                if (pos.Position != pos.LastPosition)
                {
                    ref var col = ref entity.Get<ColliderComponent>();
                    col.Rect = new System.Drawing.RectangleF(pos.Position.X, pos.Position.Y, col.Rect.Width, col.Rect.Height);
                    Game.Tree.Move(col);
                }
            }
        }
    }
}