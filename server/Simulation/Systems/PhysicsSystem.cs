using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public const int SpeedLimit = 300;
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ShapeComponent shp)
        {
            if (float.IsNaN(phy.Velocity.X))
                phy.Velocity = Vector2.Zero;

            // if(ntt.Type != EntityType.Drop)
                ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2, Game.MapSize.Y), 500);
            // ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2,0), 300);i r

            phy.AngularVelocity *= 1f - phy.Drag;
            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;

            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            phy.LastPosition = phy.Position;

            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            var newPosition = phy.Position + phy.Velocity * deltaTime;

            var size = new Vector2(shp.Radius, shp.Radius);
            newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);

            phy.Position = newPosition;

            if (phy.Velocity.Length() < 0.01 && phy.Acceleration.Length() < 0.01)
                phy.Velocity = Vector2.Zero;

            if (phy.Position.X == size.X || phy.Position.X == Game.MapSize.X - size.X)
            {
                phy.Velocity.X = -phy.Velocity.X * phy.Elasticity;
            }
            if (phy.Position.Y == size.Y || phy.Position.Y == Game.MapSize.Y - size.Y)
            {
                phy.AngularVelocity *= 0.99f;
                phy.Velocity.Y = -phy.Velocity.Y * phy.Elasticity;
                phy.Velocity.X *= 0.9f;
            }

            if (phy.AngularVelocity < 0.5)
                phy.AngularVelocity = 0f;

            phy.ChangedTick = Game.CurrentTick;

            if(phy.Position != phy.LastPosition)
                Game.Grid.Move(in ntt);
            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }

        private void ApplyGravity(ref PhysicsComponent phy, Vector2 gravityOrigin, float maxDistance)
        {
            var distance = Vector2.Distance(new Vector2(0,phy.Position.Y), new Vector2(0, gravityOrigin.Y));

            if (distance > maxDistance)
                return;

            var force = (maxDistance - distance) / maxDistance;

            phy.Acceleration += new Vector2(0,1) * force * 100 * deltaTime;
        }
    }
}