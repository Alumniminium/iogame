using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class ProjectileCollisionSystem : PixelSystem<BulletComponent, PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public ProjectileCollisionSystem() : base("Projectile Collision System", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type == EntityType.Projectile && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref BulletComponent c1, ref PhysicsComponent c2, ref ShapeComponent c3, ref ViewportComponent c4)
        {
            for (var k = 0; k < c4.EntitiesVisible.Count; k++)
            {
                var b = c4.EntitiesVisible[k];

                if (b.Id == ntt.Id || c1.Owner.Id == b.Id || b.Type == EntityType.Pickable || ntt.Type == EntityType.Pickable)
                    continue;

                ref readonly var bShp = ref b.Get<ShapeComponent>();
                ref var bPhy = ref b.Get<PhysicsComponent>();

                if (b.Type == EntityType.Projectile)
                {
                    ref readonly var bb = ref b.Get<BulletComponent>();
                    if (bb.Owner.Id == ntt.Id || bb.Owner.Id == c1.Owner.Id)
                        continue;
                    PixelWorld.Destroy(in b);
                }

                if (!(c3.Radius + bShp.Radius >= (bPhy.Position - c2.Position).Length()))
                    continue;

                var normal = Vector2.Normalize(c2.Position - bPhy.Position);
                var relVel = c2.Velocity - bPhy.Velocity;
                var sepVel = Vector2.Dot(relVel, normal);
                var newSepVel = -sepVel * Math.Min(c2.Elasticity, bPhy.Elasticity);
                var vsepDiff = newSepVel - sepVel;

                var impulse = vsepDiff / (c2.InverseMass + bPhy.InverseMass);
                var impulseVec = normal * impulse;

                var fa = impulseVec * c2.InverseMass;
                var fb = impulseVec * -bPhy.InverseMass;

                c2.Velocity += fa;
                bPhy.Velocity += fb;

                PixelWorld.Destroy(in ntt);
                var dmg = new DamageComponent(c1.Owner.Id, fb.Length());
                b.Add(ref dmg);
            }
        }
    }
}