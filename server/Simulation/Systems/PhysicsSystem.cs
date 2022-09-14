using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class PhysicsSystem : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public const int SpeedLimit = 300;
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Type != EntityType.Static && base.MatchesFilter(nttId);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ShapeComponent shp)
        {
            if (float.IsNaN(phy.Velocity.X))
                phy.Velocity = Vector2.Zero;

            ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2, Game.MapSize.Y), 300);
            // ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2, 0), 300);

            phy.AngularVelocity *= 1f - phy.Drag;
            phy.Velocity += phy.Acceleration;
            phy.Acceleration = Vector2.Zero;
            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);
            phy.Velocity *= 1f - phy.Drag;
            phy.LastPosition = phy.Position;

            phy.LastRotation = phy.RotationRadians;
            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            var newPosition = phy.Position + (phy.Velocity * deltaTime);

            var size = new Vector2(shp.Radius);
            newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);

            phy.Position = newPosition;

            if (phy.Velocity.Length() < 1 && phy.Acceleration.Length() == 0)
                phy.Velocity = Vector2.Zero;

            if (phy.Position.X == size.X || phy.Position.X == Game.MapSize.X - size.X)
            {
                phy.Velocity.X = -phy.Velocity.X * phy.Elasticity;
            }
            if (phy.Position.Y == size.Y || phy.Position.Y == Game.MapSize.Y - size.Y)
            {
                phy.AngularVelocity *= 0.99f;
                phy.Velocity.Y = -phy.Velocity.Y * phy.Elasticity;
                if (ntt.Type != EntityType.Player)
                {
                    var dmg = new DamageComponent(0, float.MaxValue);
                    ntt.Add(ref dmg);
                }
            }

            if (phy.AngularVelocity < 0.5)
                phy.AngularVelocity = 0f;

            if (phy.Position != phy.LastPosition || phy.RotationRadians != phy.LastRotation)
                phy.ChangedTick = Game.CurrentTick;
        }

        private void ApplyGravity(ref PhysicsComponent phy, Vector2 gravityOrigin, float maxDistance)
        {
            var distance = Vector2.Distance(phy.Position, gravityOrigin);

            if (distance > maxDistance)
                return;
            phy.Acceleration += new Vector2(0, 9.8f) * 10 * deltaTime;
        }
    }
}