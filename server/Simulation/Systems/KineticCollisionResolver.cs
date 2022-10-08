using Packets.Enums;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public unsafe sealed class KineticCollisionResolver : PixelSystem<CollisionComponent, PhysicsComponent>
    {
        public KineticCollisionResolver() : base("Kinetic Collision Resolver", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity a) => a.Type != EntityType.Projectile && a.Type != EntityType.Pickable && base.MatchesFilter(a);

        public override void Update(in PixelEntity a, ref CollisionComponent col, ref PhysicsComponent aPhy)
        {
            // for (int x = 0; x < col.Collisions.Count; x++)
            // {
            //     var b = col.Collisions[x].Item1;
            //     if(b.Type == EntityType.Pickable)
            //         continue;
            //     ref var bPhy = ref b.Get<PhysicsComponent>();

            //     var normal = col.Collisions[x].Item2;
            //     var depth = col.Collisions[x].Item3;
            //     var penetration = normal * depth;

            //     float e = MathF.Min(aPhy.Elasticity, bPhy.Elasticity);

            //     if (a.Type == EntityType.Static)
            //         bPhy.Position += penetration;
            //     else if (b.Type == EntityType.Static)
            //         aPhy.Position += -penetration;
            //     else
            //     {
            //         aPhy.Position += -penetration * (1 - aPhy.Mass / (aPhy.Mass + bPhy.Mass));
            //         bPhy.Position += penetration * (1 - bPhy.Mass / (aPhy.Mass + bPhy.Mass));
            //     }

            //     if (aPhy.Position != aPhy.LastPosition)
            //     {
            //         Game.Grid.Move(in a, ref aPhy);
            //         aPhy.ChangedTick = Game.CurrentTick;
            //         aPhy.TransformUpdateRequired = true;
            //     }
            //     if (bPhy.Position != bPhy.LastPosition)
            //     {
            //         Game.Grid.Move(in b, ref bPhy);
            //         bPhy.ChangedTick = Game.CurrentTick;
            //         bPhy.TransformUpdateRequired = true;
            //     }

            //     var aShieldRadius = 0f;
            //     var bShieldRadius = 0f;

            //     if (a.Has<ShieldComponent>())
            //     {
            //         ref readonly var shi = ref a.Get<ShieldComponent>();
            //         if (shi.Charge > 0)
            //             aShieldRadius = shi.Radius;
            //     }
            //     if (b.Has<ShieldComponent>())
            //     {
            //         ref readonly var shi = ref b.Get<ShieldComponent>();
            //         if (shi.Charge > 0)
            //             bShieldRadius = shi.Radius;
            //     }

            //     var impulseList = stackalloc Vector2[2];
            //     var raList = stackalloc Vector2[2];
            //     var rbList = stackalloc Vector2[2];
            //     var contactList = stackalloc Vector2[2];

            //     Collisions.FindContactPoints(ref aPhy, ref bPhy, MathF.Max(aPhy.Radius, aShieldRadius), MathF.Max(bPhy.Radius, bShieldRadius), out Vector2 contact1, out Vector2 contact2, out int contactCount);
            //     contactList[0] = contact1;
            //     contactList[1] = contact2;

            //     for (int i = 0; i < contactCount; i++)
            //     {
            //         var ra = raList[i] = contactList[i] - aPhy.Position;
            //         var rb = rbList[i] = contactList[i] - bPhy.Position;

            //         raList[i] = ra;
            //         rbList[i] = rb;

            //         var raPerp = new Vector2(-ra.Y, ra.X);
            //         var rbPerp = new Vector2(-rb.Y, rb.X);

            //         var angularVelocityA = raPerp * aPhy.AngularVelocity;
            //         var angularVelocityB = rbPerp * bPhy.AngularVelocity;

            //         var relativeVelocity = bPhy.LinearVelocity + angularVelocityB - aPhy.LinearVelocity - angularVelocityA;
            //         var contactVelocityMagnitude = Vector2.Dot(relativeVelocity, normal);

            //         if (contactVelocityMagnitude > 0)
            //             continue;

            //         var raPerpDotN = Vector2.Dot(raPerp, normal);
            //         var rbPerpDotN = Vector2.Dot(rbPerp, normal);

            //         var denom = aPhy.InvMass + bPhy.InvMass + raPerpDotN * raPerpDotN * aPhy.InvInertia + rbPerpDotN * rbPerpDotN * bPhy.InvInertia;

            //         float j = -(1 + e) * contactVelocityMagnitude / denom;
            //         j /= contactCount;

            //         impulseList[i] = normal * j;
            //     }

            //     for (int i = 0; i < contactCount; i++)
            //     {
            //         var impulse = impulseList[i];
            //         var ra = raList[i];
            //         var rb = rbList[i];

            //         if (a.Type != EntityType.Static)
            //         {
            //             var aImpact = Math.Abs((impulse * aPhy.InvMass).Length());
            //             aPhy.Acceleration += -impulse * aPhy.InvMass;
            //             aPhy.AngularVelocity += -(ra.X * impulse.Y - ra.Y * impulse.X) * aPhy.InvInertia;

            //             if (aImpact >= 2)
            //             {
            //                 var aDmg = new DamageComponent(a.Id, b.Id, aImpact / 2);
            //                 a.Add(ref aDmg);
            //             }
            //         }
            //         if (b.Type != EntityType.Static)
            //         {
            //             var bImpact = Math.Abs((impulse * bPhy.InvMass).Length());
            //             bPhy.Acceleration += impulse * bPhy.InvMass;
            //             bPhy.AngularVelocity += (rb.X * impulse.Y - rb.Y * impulse.X) * bPhy.InvInertia;

            //             if (bImpact >= 2)
            //             {
            //                 var bDmg = new DamageComponent(a.Id, a.Id, bImpact / 2);
            //                 b.Add(ref bDmg);
            //             }
            //         }
            //     }
            // }
        }
    }
}