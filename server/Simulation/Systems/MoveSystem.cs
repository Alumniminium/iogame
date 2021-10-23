using System;
using System.Numerics;
using iogame.Simulation.Entities;

namespace iogame.Simulation.Systems
{
    public static class RotationSystem
    {
        public static void Update(float deltaTime, Entity entity)
        {
            var (vel, spin, _) = entity.VelocityComponent;
            (_,_, float rot) = entity.PositionComponent;

            var radians = Math.Atan2(vel.X, vel.Y);
            rot = (float)(180 * radians / Math.PI);

            rot += spin * deltaTime;

            if (rot > 360)
                rot -= 360;
            if (rot < 0)
                rot += 360;

            entity.PositionComponent.Rotation = rot;
        }

    }
    public static class MoveSystem
    {
        public static void Update(float deltaTime, Entity entity)
        {
            var (vel, _, _) = entity.VelocityComponent;
            var (pos, _, _) = entity.PositionComponent;
            entity.PositionComponent.LastPosition = pos;

            if (entity is Player player)
            {
                var inputVector = new Vector2(0, 0);
                if (player.Left)
                    inputVector.X -= 1000;
                else if (player.Right)
                    inputVector.X += 1000;

                if (player.Up)
                    inputVector.Y -= 1000;
                else if (player.Down)
                    inputVector.Y += 1000;

                inputVector = inputVector.ClampMagnitude(1000);
                inputVector *= deltaTime;

                vel += inputVector;

                if (player.Fire)
                    player.Attack();

            }
            vel = vel.ClampMagnitude(entity.VelocityComponent.MaxSpeed);
            vel *= 1 - (entity.PhysicsComponent.Drag * deltaTime);

            if (vel.Magnitude() < 5)
                vel = Vector2.Zero;


            pos += vel * deltaTime;
            pos = Vector2.Clamp(pos, new Vector2(entity.ShapeComponent.Radius, entity.ShapeComponent.Radius), new Vector2(Game.MAP_WIDTH - entity.ShapeComponent.Radius, Game.MAP_HEIGHT - entity.ShapeComponent.Radius));

            entity.VelocityComponent.Movement = vel;
            entity.PositionComponent.Position = pos;
        }
    }
}