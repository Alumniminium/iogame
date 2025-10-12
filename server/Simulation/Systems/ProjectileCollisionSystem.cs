using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Handles bullet collision damage application and bullet destruction.
/// Stops bullet movement on impact and applies damage to collided entities.
/// </summary>
public sealed class ProjectileCollisionSystem : NttSystem<BulletComponent, PhysicsComponent, CollisionComponent, BodyDamageComponent>
{
    public ProjectileCollisionSystem() : base("Projectile Collision System", threads: 1) { }
    protected override bool MatchesFilter(in NTT a) => base.MatchesFilter(a);

    public override void Update(in NTT a, ref BulletComponent aBlt, ref PhysicsComponent aRigidBody, ref CollisionComponent col, ref BodyDamageComponent bdc)
    {
        for (int x = 0; x < col.Collisions.Count; x++)
        {
            var b = col.Collisions[x].Item1;

            var dmg = new DamageComponent(aBlt.Owner, bdc.Damage);
            b.Set(ref dmg);
            if (b.Has<PhysicsComponent>() && b.Get<PhysicsComponent>().Density == 0f)
                return;

            aRigidBody.SetLinearVelocity(Vector2.Zero);
            aRigidBody.SetAngularVelocity(0f);

            var dtc = new DeathTagComponent(default);
            a.Set(ref dtc);
        }
    }
}