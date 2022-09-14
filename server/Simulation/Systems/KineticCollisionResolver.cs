using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class KineticCollisionResolver : PixelSystem<CollisionComponent, PhysicsComponent, ShapeComponent>
    {
        public KineticCollisionResolver() : base("Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Projectile && ntt.Type != EntityType.Pickable && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref CollisionComponent c1, ref PhysicsComponent aPhy, ref ShapeComponent c3)
        {
            var b = ntt.Id == c1.A.Id ? c1.B : c1.A;

            if (b.Type == EntityType.Projectile || b.Type == EntityType.Pickable)
                return;

            ref var bPhy = ref b.Get<PhysicsComponent>();
            ref var bShp = ref b.Get<ShapeComponent>();

            var distance = aPhy.Position - bPhy.Position;
            var penetrationDepth = c3.Radius + bShp.Radius - distance.Length();
            var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (aPhy.InverseMass + bPhy.InverseMass));

            if (ntt.Type != EntityType.Static)
                aPhy.Position += penetrationResolution * aPhy.InverseMass;
            if (b.Type != EntityType.Static)
                bPhy.Position += penetrationResolution * -bPhy.InverseMass;

            var normal = Vector2.Normalize(aPhy.Position - bPhy.Position);
            var relVel = aPhy.Velocity - bPhy.Velocity;
            var sepVel = Vector2.Dot(relVel, normal);
            var newSepVel = -sepVel * MathF.Min(aPhy.Elasticity, bPhy.Elasticity);
            var vsepDiff = newSepVel - sepVel;

            var impulse = vsepDiff / (aPhy.InverseMass + bPhy.InverseMass);
            var impulseVec = normal * impulse;

            var fa = impulseVec * aPhy.InverseMass;
            var fb = impulseVec * -bPhy.InverseMass;

            if (ntt.Type != EntityType.Static)
                aPhy.Acceleration += fa;
            if (b.Type != EntityType.Static)
                bPhy.Acceleration += fb;

            var afa = fa.X >= 0 ? fa.Length() / c3.Radius : -(fa.Length() / c3.Radius);
            var afb = fb.X >= 0 ? fb.Length() / bShp.Radius : -(fb.Length() / bShp.Radius);
            aPhy.AngularVelocity += afa;
            bPhy.AngularVelocity += afb;

            var dmgToA = new DamageComponent(b.Id, MathF.Max(0.1f,fa.Length()));
            var dmgToB = new DamageComponent(ntt.Id, MathF.Max(0.1f,fb.Length()));

            if (ntt.Type != EntityType.Static)
                ntt.Add(ref dmgToA);
            if (b.Type != EntityType.Static)
                b.Add(ref dmgToB);
        }
    }
}