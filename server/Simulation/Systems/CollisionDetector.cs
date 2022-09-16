using System;
using System.Numerics;
using FlatPhysics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class CollisionDetector : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity a, ref PhysicsComponent bodyA, ref ViewportComponent vwp)
        {
            for (var k = 0; k < vwp.EntitiesVisible.Length; k++)
            {
                var b = vwp.EntitiesVisible[k];

                if (b.Id == a.Id || b.Has<CollisionComponent>())
                    continue;

                ref var bodyB = ref b.Get<PhysicsComponent>();

                if (Collisions.Collide(ref bodyA, ref bodyB, out Vector2 normal, out float depth))
                {
                    var penetration = normal * depth;
                    if (a.Type == EntityType.Static)
                        bodyB.Move(penetration);
                    else if (b.Type == EntityType.Static)
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

                    bodyA.Acceleration -= impulse * bodyA.InvMass * deltaTime;
                    bodyB.Acceleration += impulse * bodyB.InvMass * deltaTime;
                }
            }
        }
    }
}