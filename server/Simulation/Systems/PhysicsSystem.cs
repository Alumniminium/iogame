using System;
using System.Diagnostics;
using System.Numerics;
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
            var time = deltaTime / 10;
            for (int i = 0; i < 10; i++)
            {
                if (bodyA.IsStatic)
                    return;
                if (float.IsNaN(bodyA.LinearVelocity.X))
                    bodyA.LinearVelocity = Vector2.Zero;

                ApplyGravity(ref bodyA, new Vector2(Game.MapSize.X / 2, Game.MapSize.Y), 300);

                bodyA.RotationalVelocity *= 1f - bodyA.Drag;
                bodyA.LinearVelocity += bodyA.Acceleration;
                bodyA.LinearVelocity = bodyA.LinearVelocity.ClampMagnitude(SpeedLimit);
                bodyA.LinearVelocity *= 1f - bodyA.Drag;
                bodyA.LastPosition = bodyA.Position;

                bodyA.LastRotation = bodyA.Rotation;
                bodyA.Rotation += bodyA.RotationalVelocity * time;
                var newPosition = bodyA.Position + (bodyA.LinearVelocity * time);

                var size = new Vector2(bodyA.Radius);
                newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);

                bodyA.Position = newPosition;

                if (bodyA.LinearVelocity.Length() < 1 && bodyA.Acceleration.Length() == 0)
                    bodyA.LinearVelocity = Vector2.Zero;

                if (bodyA.Position.X == size.X || bodyA.Position.X == Game.MapSize.X - size.X)
                {
                    bodyA.LinearVelocity.X = -bodyA.LinearVelocity.X * bodyA.Restitution;
                }
                if (bodyA.Position.Y == size.Y || bodyA.Position.Y == Game.MapSize.Y - size.Y)
                {
                    bodyA.RotationalVelocity *= 0.99f;
                    bodyA.LinearVelocity.Y = -bodyA.LinearVelocity.Y * bodyA.Restitution;
                    if (a.Type != EntityType.Player)
                    {
                        var dmg = new DamageComponent(0, float.MaxValue);
                        a.Add(ref dmg);
                    }
                }
                bodyA.Acceleration = Vector2.Zero;

                if (bodyA.RotationalVelocity < 0.5)
                    bodyA.RotationalVelocity = 0f;

                ref readonly var vwp = ref a.Get<ViewportComponent>();

                for (var k = 0; k < vwp.EntitiesVisible.Length; k++)
                {
                    var b = vwp.EntitiesVisible[k];

                    if (b.Id == a.Id || b.Has<CollisionComponent>())
                        continue;

                    ref var bodyB = ref b.Get<PhysicsComponent>();

                    if (Collisions.Collide(ref bodyA, ref bodyB, out Vector2 normal, out float depth))
                    {
                        var penetration = normal * depth;
                        if (bodyA.IsStatic)
                            bodyB.Move(penetration);
                        else if (bodyB.IsStatic)
                            bodyA.Move(-penetration);
                        else
                        {
                            bodyA.Move(-penetration / 2f);
                            bodyB.Move(penetration / 2f);
                        }
                        Vector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

                        if (Vector2.Dot(relativeVelocity, normal) > 0f)
                            return;

                        float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

                        float j = -(1f + e) * Vector2.Dot(relativeVelocity, normal);
                        j /= bodyA.InvMass + bodyB.InvMass;

                        Vector2 impulse = j * normal;

                        bodyA.Acceleration -= impulse * bodyA.InvMass;
                        bodyB.Acceleration += impulse * bodyB.InvMass;
                    }
                }

                if (bodyA.Position != bodyA.LastPosition || bodyA.Rotation != bodyA.LastRotation)
                {
                    if (float.IsNaN(bodyA.Position.X))
                        Debugger.Break();
                    bodyA.TransformUpdateRequired = true;
                    bodyA.AabbUpdateRequired = true;
                    bodyA.ChangedTick = Game.CurrentTick;
                    Game.Grid.Move(a);
                }
            }
        }

        private void ApplyGravity(ref PhysicsComponent phy, Vector2 gravityOrigin, float maxDistance)
        {
            var distance = Vector2.Distance(phy.Position, gravityOrigin);

            if (distance > maxDistance)
                return;
            phy.Acceleration += new Vector2(0, 9.8f) * deltaTime;
        }
    }
}