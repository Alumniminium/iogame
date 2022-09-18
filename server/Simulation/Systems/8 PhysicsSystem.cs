using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using FlatPhysics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 300;
        public PhysicsSystem() : base("Physics System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity nttId) => nttId.Type != EntityType.Static && base.MatchesFilter(nttId);

        public override void Update(in PixelEntity a, ref PhysicsComponent bodyA)
        {
            if (a.Type == EntityType.Static)
                return;
            
            ApplyGravity(ref bodyA, new Vector2(Game.MapSize.X / 2, Game.MapSize.Y), 500, 1);

            if(bodyA.Acceleration == Vector2.Zero && bodyA.LinearVelocity == Vector2.Zero)
                return;
            var size = bodyA.ShapeType == ShapeType.Circle ? new Vector2(bodyA.Radius) : new Vector2(bodyA.Width, bodyA.Height);

            bodyA.LastPosition = bodyA.Position;
            bodyA.LastRotation = bodyA.RotationRadians;
            bodyA.RotationRadians += bodyA.AngularVelocity * deltaTime;
            bodyA.AngularVelocity *= 1f - bodyA.Drag;

            bodyA.LinearVelocity += bodyA.Acceleration;
            bodyA.LinearVelocity = bodyA.LinearVelocity.ClampMagnitude(SpeedLimit);
            bodyA.LinearVelocity *= 1f - bodyA.Drag;


            bodyA.Acceleration = Vector2.Zero;
                
            if (bodyA.LinearVelocity.Length() < 0.1)
                bodyA.LinearVelocity = Vector2.Zero;
                
            var newPosition = bodyA.Position + (bodyA.LinearVelocity * deltaTime);
            newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);
            bodyA.Position = newPosition;

            if (bodyA.Position.X == size.X || bodyA.Position.X == Game.MapSize.X - size.X)
                bodyA.LinearVelocity.X = -bodyA.LinearVelocity.X * bodyA.Elasticity;
            
            if (bodyA.Position.Y == size.Y || bodyA.Position.Y == Game.MapSize.Y - size.Y)
            {
                bodyA.LinearVelocity.Y = -bodyA.LinearVelocity.Y * bodyA.Elasticity;
                if (a.Type != EntityType.Player)
                    a.Add<DeathTagComponent>();
            }

            if (bodyA.Position != bodyA.LastPosition || bodyA.RotationRadians != bodyA.LastRotation)
            {
                bodyA.TransformUpdateRequired = true;
                bodyA.ChangedTick = Game.CurrentTick;
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