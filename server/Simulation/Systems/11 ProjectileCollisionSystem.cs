using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class ProjectileCollisionSystem : PixelSystem<BulletComponent, PhysicsComponent, CollisionComponent, BodyDamageComponent>
    {
        public ProjectileCollisionSystem() : base("Projectile Collision System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity a) => a.Type == EntityType.Projectile && base.MatchesFilter(a);

        public override void Update(in PixelEntity a, ref BulletComponent aBlt, ref PhysicsComponent aPhy, ref CollisionComponent col, ref BodyDamageComponent bdc)
        {
            if (!col.EntityTypes.HasFlag(EntityType.Projectile))
                return;
            if (col.EntityTypes.HasFlag(EntityType.Pickable))
                return;

            var b = a.Id == col.A.Id ? col.B : col.A;

            if (aBlt.Owner.Id == b.Id)
                return;




        }
    }
}