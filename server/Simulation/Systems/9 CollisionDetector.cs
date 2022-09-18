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

                if (b.Id == a.Id)
                    continue;

                ref var bodyB = ref b.Get<PhysicsComponent>();
                var aShieldRadius = 0f;
                var bShieldRadius = 0f;

                if (a.Has<ShieldComponent>())
                    aShieldRadius = a.Get<ShieldComponent>().Radius;
                if (b.Has<ShieldComponent>())
                    bShieldRadius = b.Get<ShieldComponent>().Radius;

                if (a.Type == EntityType.Projectile && b.Type == EntityType.Projectile)
                {
                    ref readonly var bulletA = ref a.Get<BulletComponent>();
                    ref readonly var bulletB = ref b.Get<BulletComponent>();

                    if (bulletA.Owner.Id == bulletB.Owner.Id)
                        continue;
                }
                else if (a.Type == EntityType.Projectile)
                {
                    ref readonly var bullet = ref a.Get<BulletComponent>();
                    if (bullet.Owner.Id == b.Id)
                        continue;
                }
                else if (b.Type == EntityType.Projectile)
                {
                    ref readonly var bullet = ref b.Get<BulletComponent>();
                    if (bullet.Owner.Id == a.Id)
                        continue;
                }

                if (Collisions.Collide(ref bodyA, ref bodyB, aShieldRadius, bShieldRadius, out Vector2 normal, out float depth))
                {
                    var penetration = normal * MathF.Max(0.01f, depth);

                    if (a.Type == EntityType.Static)
                        bodyB.Position += penetration;
                    else if (b.Type == EntityType.Static)
                        bodyA.Position += -penetration;
                    else
                    {
                        bodyA.Position += -penetration / 2f;
                        bodyB.Position += penetration / 2f;
                    }
                    Vector2 deltaV = bodyB.LinearVelocity - bodyA.LinearVelocity;

                    float e = MathF.Min(bodyA.Elasticity, bodyB.Elasticity);

                    float j = -(1f + e) * Vector2.Dot(deltaV, normal);
                    j /= bodyA.InvMass + bodyB.InvMass;

                    Vector2 impulse = j * normal * 1f;

                    bodyA.Acceleration -= impulse * bodyA.InvMass;
                    bodyB.Acceleration += impulse * bodyB.InvMass;

                    bodyB.TransformUpdateRequired = true;
                    bodyA.TransformUpdateRequired = true;
                    bodyA.ChangedTick = Game.CurrentTick;
                    bodyB.ChangedTick = Game.CurrentTick;

                    if (a.Type == EntityType.Player || b.Type == EntityType.Player)
                    {
                        var col = new CollisionComponent(a, b, impulse);
                        a.Add(ref col);
                    }
                }
            }
        }
    }
}