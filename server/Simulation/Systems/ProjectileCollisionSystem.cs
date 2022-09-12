using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class ProjectileCollisionSystem : PixelSystem<BulletComponent, PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public ProjectileCollisionSystem() : base("Projectile Collision System", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt)
        {
            return ntt.IsBullet() && base.MatchesFilter(ntt);
        }

        public override void Update(in PixelEntity a, ref BulletComponent ab, ref PhysicsComponent aPhy, ref ShapeComponent aShp, ref ViewportComponent aVwp)
        {
            for (var k = 0; k < aVwp.EntitiesVisible.Count; k++)
            {
                ref readonly var b = ref aVwp.EntitiesVisible[k].Entity;

                if (b.Id == a.Id || ab.Owner.Id == b.Id || b.IsDrop())
                    continue;

                ref readonly var bShp = ref b.Get<ShapeComponent>();
                ref var bPhy = ref b.Get<PhysicsComponent>();

                if (b.IsBullet())
                {
                    ref readonly var bb = ref b.Get<BulletComponent>();
                    if (bb.Owner.Id == a.Id || bb.Owner.Id == ab.Owner.Id)
                        continue;
                    PixelWorld.Destroy(in b);
                }

                if (!(aShp.Radius + bShp.Radius >= (bPhy.Position - aPhy.Position).Length()))
                    continue;

                var normal = Vector2.Normalize(aPhy.Position - bPhy.Position);
                var relVel = aPhy.Velocity - bPhy.Velocity;
                var sepVel = Vector2.Dot(relVel, normal);
                var newSepVel = -sepVel * Math.Min(aPhy.Elasticity, bPhy.Elasticity);
                var vsepDiff = newSepVel - sepVel;

                var impulse = vsepDiff / (aPhy.InverseMass + bPhy.InverseMass);
                var impulseVec = normal * impulse;

                var fa = impulseVec * aPhy.InverseMass;
                var fb = impulseVec * -bPhy.InverseMass;

                aPhy.Velocity += fa;
                bPhy.Velocity += fb;

                PixelWorld.Destroy(in a);
                var dmg = new DamageComponent(ab.Owner.Id, fb.Length());
                b.Add(ref dmg);
            }
        }
    }
}