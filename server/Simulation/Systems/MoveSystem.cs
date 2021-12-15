using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class MoveSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent, SpeedComponent>
    {
        public const int SpeedLimit = 1750;
        public MoveSystem() : base("Move System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref readonly var phy = ref entity.Get<PhysicsComponent>();
                ref readonly var spd = ref entity.Get<SpeedComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();

                vel.Velocity += vel.Acceleration;
                vel.Velocity = vel.Velocity.ClampMagnitude(Math.Min(SpeedLimit, spd.Speed));

                vel.Velocity *= 1f - phy.Drag;

                if (vel.Velocity.Length() < 0.25)
                    vel.Velocity = Vector2.Zero;

                pos.LastPosition = pos.Position;
                var newPosition = pos.Position + vel.Velocity * dt;
                pos.Rotation = (float)Math.Atan2(newPosition.Y - pos.Position.Y, newPosition.X - pos.Position.X);
                pos.Position = Vector2.Clamp(newPosition, Vector2.Zero, Game.MapSize);

                if(pos.Position == pos.LastPosition)
                    continue;
                
                var shpEntity = PixelWorld.GetAttachedShapeEntity(in entity);
                shpEntity.Rect = new(pos.Position.X - shpEntity.Rect.Width / 2, pos.Position.Y - shpEntity.Rect.Height / 2, shpEntity.Rect.Width, shpEntity.Rect.Height);
                    
                lock (Game.Tree)
                    Game.Tree.Move(shpEntity);
            }
        }
    }
}