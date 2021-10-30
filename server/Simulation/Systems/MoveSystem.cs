using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using iogame.Simulation.Entities;
using iogame.Util;
using Microsoft.VisualBasic;

namespace iogame.Simulation.Systems
{
    public static class MoveSystem
    {
        static MoveSystem() => PerformanceMetrics.RegisterSystem(nameof(MoveSystem));
        public static unsafe void Update(float deltaTime, Entity entity)
        {
            var (vel, _, _) = entity.VelocityComponent;
            var pos = entity.PositionComponent.Position;
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
            vel *= 1f - (entity.PhysicsComponent.Drag * deltaTime);

            if (vel.Magnitude() < 5)
                vel = Vector2.Zero;


            pos += vel * deltaTime;
            pos = Vector2.Clamp(pos, new Vector2(entity.ShapeComponent.Radius, entity.ShapeComponent.Radius), new Vector2(Game.MAP_WIDTH - entity.ShapeComponent.Radius, Game.MAP_HEIGHT - entity.ShapeComponent.Radius));

            entity.VelocityComponent.Movement = vel;
            entity.PositionComponent.Position = pos;
        }
    }
}