using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class ProjectileCollisionSystem : PixelSystem<BulletComponent, PhysicsComponent, CollisionComponent, BodyDamageComponent>
    {
        public ProjectileCollisionSystem() : base("Projectile Collision System", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type == EntityType.Projectile && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity a, ref BulletComponent aBlt, ref PhysicsComponent aPhy, ref CollisionComponent c4, ref BodyDamageComponent bdc)
        {
            var b = a.Id == c4.A.Id ? c4.B : c4.A;

            if(b.Type == EntityType.Static)
            {
                var dtc = new DeathTagComponent(0);
                a.Add(ref dtc);
            }

            if(b.Type == EntityType.Projectile)
            {
                ref readonly var bBlt = ref b.Get<BulletComponent>();
                if(aBlt.Owner.Id == bBlt.Owner.Id)
                    return;
                
                var dtc = new DeathTagComponent(0);
                b.Add(ref dtc);
                a.Add(ref dtc);
            }
            else
            {
                ref var bPhy = ref b.Get<PhysicsComponent>();

                var normal = Vector2.Normalize(aPhy.Position - bPhy.Position);
                var relVel = aPhy.Velocity - bPhy.Velocity;
                var sepVel = Vector2.Dot(relVel, normal);
                var newSepVel = -sepVel * Math.Min(aPhy.Elasticity, bPhy.Elasticity);
                var vsepDiff = newSepVel - sepVel;

                var impulse = vsepDiff / (aPhy.InverseMass + bPhy.InverseMass);
                var impulseVec = normal * impulse;

                var fa = impulseVec * aPhy.InverseMass;
                var fb = impulseVec * -bPhy.InverseMass;

                aPhy.Acceleration += fa;
                bPhy.Acceleration += fb;
            }

            var dmg = new DamageComponent(aBlt.Owner.Id, bdc.Damage);
            b.Add(ref dmg);
            a.Add(ref dmg);
        }
    }
}