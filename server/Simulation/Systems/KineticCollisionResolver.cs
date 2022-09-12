using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class KineticCollisionResolver : PixelSystem<CollisionComponent, PhysicsComponent, ShapeComponent>
    {
        public KineticCollisionResolver() : base("Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Bullet && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref CollisionComponent c1, ref PhysicsComponent c2, ref ShapeComponent c3)
        {
            var b = ntt.Id == c1.A.Id ? c1.B : c1.A;

            if (b.Type  == EntityType.Bullet)
                return;

            ref var bPhy = ref b.Get<PhysicsComponent>();
            ref var bShp = ref b.Get<ShapeComponent>();

            var distance = c2.Position - bPhy.Position;
            var penetrationDepth = c3.Radius + bShp.Radius - distance.Length();
            var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (c2.InverseMass + bPhy.InverseMass));
            c2.Position += penetrationResolution * c2.InverseMass;
            bPhy.Position += penetrationResolution * -bPhy.InverseMass;

            var normal = Vector2.Normalize(c2.Position - bPhy.Position);
            var relVel = c2.Velocity - bPhy.Velocity;
            var sepVel = Vector2.Dot(relVel, normal);
            var newSepVel = -sepVel * MathF.Min(c2.Elasticity, bPhy.Elasticity);
            var vsepDiff = newSepVel - sepVel;

            var impulse = vsepDiff / (c2.InverseMass + bPhy.InverseMass);
            var impulseVec = normal * impulse;

            var fa = impulseVec * c2.InverseMass;
            var fb = impulseVec * -bPhy.InverseMass;

            c2.Velocity += fa;
            bPhy.Velocity += fb;

            var afa = fa.X >= 0 ? fa.Length() / c3.Radius : -(fa.Length() / c3.Radius);
            var afb = fb.X >= 0 ? fb.Length() / bShp.Radius : -(fb.Length() / bShp.Radius);
            c2.AngularVelocity += afa;
            bPhy.AngularVelocity += afb;

            if (fa.Length() > 0)
            {
                var dmgToA = new DamageComponent(b.Id, fa.Length());
                ntt.Add(ref dmgToA);
            }
            if (fb.Length() > 0)
            {
                var dmgToB = new DamageComponent(ntt.Id, fb.Length());
                b.Add(ref dmgToB);
            }
            ntt.Remove<CollisionComponent>();
            b.Remove<CollisionComponent>();
        }
    }
}