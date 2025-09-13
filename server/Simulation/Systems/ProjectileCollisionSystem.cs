using server.ECS;
using server.Enums;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class ProjectileCollisionSystem : PixelSystem<BulletComponent, PhysicsComponent, CollisionComponent, BodyDamageComponent>
{
    public ProjectileCollisionSystem() : base("Projectile Collision System", threads: 1) { }
    protected override bool MatchesFilter(in PixelEntity a) => a.Type == EntityType.Projectile && base.MatchesFilter(a);

    public override void Update(in PixelEntity a, ref BulletComponent aBlt, ref PhysicsComponent aPhy, ref CollisionComponent col, ref BodyDamageComponent bdc)
    {
        for (int x = 0; x < col.Collisions.Count; x++)
        {
            var b = col.Collisions[x].Item1;

            var dmg = new DamageComponent(a.Id, aBlt.Owner.Id, bdc.Damage);
            b.Add(ref dmg);
            if (b.Type == EntityType.Static || b.Type == EntityType.Pickable)
                return;

            var dtc = new DeathTagComponent(a.Id, 0);
            a.Add(ref dtc);
        }
    }
}