using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Systems
{
    public sealed class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 300;
        public PhysicsSystem() : base("Physics System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Type != EntityType.Static && base.MatchesFilter(nttId);

        public override void Update(in PixelEntity a, ref PhysicsComponent phy)
        {
            if (a.Type == EntityType.Static)
                return;
            
            ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2, Game.MapSize.Y), 900, 1);

            if (phy.Acceleration == Vector2.Zero && phy.LinearVelocity == Vector2.Zero && phy.AngularVelocity == 0 && phy.Position == phy.LastPosition)
                return;

            var size = phy.ShapeType == ShapeType.Circle ? new Vector2(phy.Radius) : new Vector2(phy.Width, phy.Height);

            phy.LastPosition = phy.Position;
            phy.LastRotation = phy.RotationRadians;
            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            phy.AngularVelocity *= 1f - phy.Drag;

            if(phy.AngularVelocity < 0.1)
                phy.AngularVelocity = 0;

            phy.LinearVelocity += phy.Acceleration;
            phy.LinearVelocity = phy.LinearVelocity.ClampMagnitude(SpeedLimit);
            phy.LinearVelocity *= 1f - phy.Drag;

            phy.Acceleration = Vector2.Zero;

            if (phy.LinearVelocity.Length() < 1)
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
                    a.Add<DeathTagComponent>();
            }
            if(phy.RotationRadians != phy.LastRotation)
                phy.ChangedTick = Game.CurrentTick;

            if (phy.Position != phy.LastPosition)
            {
                phy.TransformUpdateRequired = true;
                phy.ChangedTick = Game.CurrentTick;
                Game.Grid.Move(a);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyGravity(ref PhysicsComponent phy, Vector2 gravityOrigin, float maxDistance, int iterations)
        {
            var distance = Vector2.Distance(phy.Position, gravityOrigin);

            if (distance > maxDistance)
                return;
            phy.Acceleration += new Vector2(0, 9.8f) * 10 * (deltaTime / iterations);
        }
    }
}