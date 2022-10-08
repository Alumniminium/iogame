using System;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 400;
        public PhysicsSystem() : base("Physics System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Type != EntityType.Static && base.MatchesFilter(nttId);

        public override void Update(in PixelEntity a, ref PhysicsComponent phy)
        {
            if (phy.Position.Y > Game.MapSize.Y - 1500)
                phy.Acceleration += new Vector2(0, 9.81f) * deltaTime;

            if (float.IsNaN(phy.Acceleration.X) || float.IsNaN(phy.Acceleration.Y))
                phy.Acceleration = Vector2.Zero;

            if (phy.Acceleration == Vector2.Zero && phy.LinearVelocity == Vector2.Zero && phy.AngularVelocity == 0 && phy.Position == phy.LastPosition)
                return;

            var size = phy.ShapeType == ShapeType.Circle ? new Vector2(phy.Radius) : new Vector2(phy.Width, phy.Height);

            phy.LastPosition = phy.Position;
            phy.LastRotation = phy.RotationRadians;
            phy.RotationRadians += phy.AngularVelocity * deltaTime;            
            phy.AngularVelocity *= 1f - phy.Drag;

            if(phy.RotationRadians > MathF.PI * 2)
                phy.RotationRadians -= MathF.PI * 2;
            if(phy.RotationRadians < 0)
                phy.RotationRadians += MathF.PI * 2;

            // phy.AngularVelocity = 1f;

            if (MathF.Abs(phy.AngularVelocity) < 0.1)
                phy.AngularVelocity = 0;

            phy.LinearVelocity += phy.Acceleration;
            phy.LinearVelocity = phy.LinearVelocity.ClampMagnitude(SpeedLimit);
            phy.LinearVelocity *= 1f - phy.Drag;

            phy.Acceleration = Vector2.Zero;

            if (float.IsNaN(phy.LinearVelocity.X) || float.IsNaN(phy.LinearVelocity.Y))
                phy.LinearVelocity = Vector2.Zero;

            if (phy.LinearVelocity.Length() < 0.1)
                phy.LinearVelocity = Vector2.Zero;

            var newPosition = phy.Position + (phy.LinearVelocity * deltaTime);
            newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);
            phy.Position = newPosition;

            if (phy.Position.X == size.X || phy.Position.X == Game.MapSize.X - size.X)
                phy.LinearVelocity.X = -phy.LinearVelocity.X * phy.Elasticity;

            if (phy.Position.Y == size.Y || phy.Position.Y == Game.MapSize.Y - size.Y)
            {
                phy.LinearVelocity.Y = -phy.LinearVelocity.Y * phy.Elasticity;
                if (a.Type != EntityType.Player)
                {
                    var dtc = new DeathTagComponent(a.Id,0);
                    a.Add(ref dtc);
                }
            }
            if (phy.RotationRadians != phy.LastRotation)
            {
                phy.ChangedTick = Game.CurrentTick;
                phy.TransformUpdateRequired = true;
            }
            if (phy.Position != phy.LastPosition)
            {
                phy.TransformUpdateRequired = true;
                phy.ChangedTick = Game.CurrentTick;
                Game.Grid.Move(in a, ref phy);              
            }
        }
    }
}