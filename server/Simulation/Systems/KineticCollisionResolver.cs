using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class KineticCollisionResolver : PixelSystem<CollisionComponent, PhysicsComponent>
    {
        public KineticCollisionResolver() : base("Collision Resolver", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity a) => a.Type != EntityType.Projectile && a.Type != EntityType.Pickable && base.MatchesFilter(a);

        public override void Update(in PixelEntity a, ref CollisionComponent col, ref PhysicsComponent aPhy)
        {
            if (col.EntityTypes.HasFlag(EntityType.Projectile) || col.EntityTypes.HasFlag(EntityType.Pickable))
                return;

            var b = a.Id == col.A.Id ? col.B : col.A;

        }
    }
}