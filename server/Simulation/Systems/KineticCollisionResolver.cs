using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class KineticCollisionResolver : PixelSystem<CollisionComponent, PhysicsComponent, ShapeComponent>
    {
        public KineticCollisionResolver() : base("Collision Resolver", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => !ntt.IsBullet() && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity a, ref CollisionComponent col, ref PhysicsComponent aPhy, ref ShapeComponent aShp)
        {
            var b = a.Id == col.A.Id ? col.B : col.A;
            ref var bPhy = ref b.Get<PhysicsComponent>();
            ref var bShp = ref b.Get<ShapeComponent>();

            var distance = aPhy.Position - bPhy.Position;
            var penetrationDepth = aShp.Radius + bShp.Radius - distance.Length();
            var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (aPhy.InverseMass + bPhy.InverseMass));
            aPhy.Position += penetrationResolution * aPhy.InverseMass;
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

            aPhy.Velocity += fa;
            bPhy.Velocity += fb;

            var afa = fa.X >= 0 ? fa.Length() / aShp.Radius : -(fa.Length() / aShp.Radius);
            var afb = fb.X >= 0 ? fb.Length() / bShp.Radius : -(fb.Length() / bShp.Radius);
            aPhy.AngularVelocity += afa;
            bPhy.AngularVelocity += afb;

            if (fa.Length() > 0)
            {
                var dmgToA = new DamageComponent(b.Id, fa.Length());
                a.Add(ref dmgToA);
            }
            if (fb.Length() > 0)
            {
                var dmgToB = new DamageComponent(a.Id, fb.Length());
                b.Add(ref dmgToB);
            }
            a.Remove<CollisionComponent>();
            b.Remove<CollisionComponent>();
        }
    }
}