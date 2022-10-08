using System;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public unsafe sealed class CollisionDetector : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity a, ref PhysicsComponent bodyA, ref ViewportComponent vwp)
        {
            if (bodyA.LastPosition == bodyA.Position)
                return;
            if (a.Type == EntityType.Static || a.Type == EntityType.Pickable)
                return;

            for (var k = 0; k < vwp.EntitiesVisible.Count; k++)
            {
                if (vwp.EntitiesVisible[k].Id == 0)
                    continue;

                var b = vwp.EntitiesVisible[k];

                if (b.Id == a.Id)
                    continue;

                ref var bodyB = ref b.Get<PhysicsComponent>();

                var aShieldRadius = 0f;
                var bShieldRadius = 0f;

                if (a.Has<ShieldComponent>())
                {
                    ref readonly var shi = ref a.Get<ShieldComponent>();
                    if (shi.Charge > 0)
                        aShieldRadius = shi.Radius;
                }
                if (b.Has<ShieldComponent>())
                {
                    ref readonly var shi = ref b.Get<ShieldComponent>();
                    if (shi.Charge > 0)
                        bShieldRadius = shi.Radius;
                }

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

                if (Collisions.Collide(ref bodyA, ref bodyB, MathF.Max(bodyA.Radius, aShieldRadius), MathF.Max(bodyB.Radius, bShieldRadius), out Vector2 normal, out float depth))
                {
                    Collisions.FindContactPoints(ref bodyA, ref bodyB, MathF.Max(bodyA.Radius, aShieldRadius), MathF.Max(bodyB.Radius, bShieldRadius), out Vector2 contact1, out Vector2 contact2, out int contactCount);
                    var penetration = normal * depth;
                    float e = MathF.Min(bodyA.Elasticity, bodyB.Elasticity);

                    if (a.Type == EntityType.Static)
                        bodyB.Position += penetration;
                    else if (b.Type == EntityType.Static)
                        bodyA.Position += -penetration;
                    else
                    {
                        bodyA.Position += -penetration * (1 - bodyA.Mass / (bodyA.Mass + bodyB.Mass));
                        bodyB.Position += penetration * (1 - bodyB.Mass / (bodyA.Mass + bodyB.Mass));
                    }

                    var impulseList = stackalloc Vector2[2];
                    var raList = stackalloc Vector2[2];
                    var rbList = stackalloc Vector2[2];
                    var contactList = stackalloc Vector2[2];
                    contactList[0] = contact1;
                    contactList[1] = contact2;

                    // for (int i = 0; i < contactCount; i++)
                    // {
                    //     impulseList[i] = Vector2.Zero;
                    //     raList[i] = Vector2.Zero;
                    //     rbList[i] = Vector2.Zero;
                    // }

                    for (int i = 0; i < contactCount; i++)
                    {
                        var ra = raList[i] = contactList[i] - bodyA.Position;
                        var rb = rbList[i] = contactList[i] - bodyB.Position;

                        raList[i] = ra;
                        rbList[i] = rb;

                        var raPerp = new Vector2(-ra.Y, ra.X);
                        var rbPerp = new Vector2(-rb.Y, rb.X);

                        var angularVelocityA = raPerp * bodyA.AngularVelocity;
                        var angularVelocityB = rbPerp * bodyB.AngularVelocity;

                        var relativeVelocity = bodyB.LinearVelocity + angularVelocityB - bodyA.LinearVelocity - angularVelocityA;
                        var contactVelocityMagnitude = Vector2.Dot(relativeVelocity, normal);

                        if (contactVelocityMagnitude > 0)
                            continue;

                        var raPerpDotN = Vector2.Dot(raPerp, normal);
                        var rbPerpDotN = Vector2.Dot(rbPerp, normal);

                        var denom = bodyA.InvMass + bodyB.InvMass + raPerpDotN * raPerpDotN * bodyA.InvInertia + rbPerpDotN * rbPerpDotN * bodyB.InvInertia;

                        float j = -(1 + e) * contactVelocityMagnitude / denom;
                        j /= contactCount;

                        impulseList[i] = normal * j;
                    }

                    for (int i = 0; i < contactCount; i++)
                    {
                        var impulse = impulseList[i];
                        var ra = raList[i];
                        var rb = rbList[i];

                        if (a.Type != EntityType.Static)
                            bodyA.Acceleration -= impulse * bodyA.InvMass;
                        if (b.Type != EntityType.Static)
                            bodyB.Acceleration += impulse * bodyB.InvMass;

                        if (a.Type != EntityType.Static)
                            bodyA.AngularVelocity -= (ra.X * impulse.Y - ra.Y * impulse.X) * bodyA.InvInertia;
                        if (b.Type != EntityType.Static)
                            bodyB.AngularVelocity += (rb.X * impulse.Y - rb.Y * impulse.X) * bodyB.InvInertia;

                        var col = new CollisionComponent(a, b, impulse);
                        a.Add(ref col);
                        b.Add(ref col);
                    }


                    if (bodyA.Position != bodyA.LastPosition)
                    {
                        Game.Grid.Move(in a, ref bodyA);
                        bodyA.ChangedTick = Game.CurrentTick;
                        bodyA.TransformUpdateRequired = true;
                    }
                    if (bodyB.Position != bodyB.LastPosition)
                    {
                        Game.Grid.Move(in b, ref bodyB);
                        bodyB.ChangedTick = Game.CurrentTick;
                        bodyB.TransformUpdateRequired = true;
                    }
                }
            }
        }
    }
}