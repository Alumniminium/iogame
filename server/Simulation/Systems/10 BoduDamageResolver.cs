using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class BodyDamageResolver : PixelSystem<CollisionComponent, PhysicsComponent>
    {
        public BodyDamageResolver() : base("Body Damage Resolver", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity a) => a.Type != EntityType.Projectile && a.Type != EntityType.Pickable && base.MatchesFilter(a);

        public override void Update(in PixelEntity a, ref CollisionComponent col, ref PhysicsComponent aPhy)
        {
            if (col.EntityTypes.HasFlag(EntityType.Projectile) || col.EntityTypes.HasFlag(EntityType.Pickable))
                return;

            var b = a.Id == col.A.Id ? col.B : col.A;
            ref var bPhy = ref b.Get<PhysicsComponent>();
            
            var bImpact = MathF.Abs(col.Impulse.Length() * bPhy.InvMass);
            if (bImpact >= 2)
            {
                var bDmg = new DamageComponent(a.Id, bImpact);
                b.Add(ref bDmg);
            }

            var aImpact = MathF.Abs(col.Impulse.Length() * aPhy.InvMass);
            if (aImpact >= 2)
            {
                var aDmg = new DamageComponent(b.Id, aImpact);
                a.Add(ref aDmg);
            }
        }
    }
}