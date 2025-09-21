using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class ProjectileCollisionSystem : NttSystem<BulletComponent, PhysicsComponent, CollisionComponent, BodyDamageComponent>
{
    public ProjectileCollisionSystem() : base("Projectile Collision System", threads: 1) { }
    protected override bool MatchesFilter(in NTT a) => base.MatchesFilter(a);

    public override void Update(in NTT a, ref BulletComponent aBlt, ref PhysicsComponent aPhy, ref CollisionComponent col, ref BodyDamageComponent bdc)
    {
        for (int x = 0; x < col.Collisions.Count; x++)
        {
            var b = col.Collisions[x].Item1;

            var dmg = new DamageComponent(a, aBlt.Owner, bdc.Damage);
            b.Set(ref dmg);
            if (b.Get<PhysicsComponent>().Static)
                return;

            var dtc = new DeathTagComponent(a, default);
            a.Set(ref dtc);
        }
    }
}