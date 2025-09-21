using System;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class ProjectileCollisionSystem : NttSystem<BulletComponent, Box2DBodyComponent, CollisionComponent, BodyDamageComponent>
{
    public ProjectileCollisionSystem() : base("Projectile Collision System", threads: 1) { }
    protected override bool MatchesFilter(in NTT a) => base.MatchesFilter(a);

    public override void Update(in NTT a, ref BulletComponent aBlt, ref Box2DBodyComponent aRigidBody, ref CollisionComponent col, ref BodyDamageComponent bdc)
    {
        Console.WriteLine($"🔥 ProjectileCollisionSystem processing bullet {a.Id} with {col.Collisions.Count} collisions");

        for (int x = 0; x < col.Collisions.Count; x++)
        {
            var b = col.Collisions[x].Item1;

            Console.WriteLine($"💣 Bullet {a.Id} applying {bdc.Damage} damage to {b.Id}");

            var dmg = new DamageComponent(a, aBlt.Owner, bdc.Damage);
            b.Set(ref dmg);
            if (b.Has<Box2DBodyComponent>() && b.Get<Box2DBodyComponent>().Density == 0f)
                return;

            // Stop bullet movement immediately to prevent bouncing
            aRigidBody.SetLinearVelocity(Vector2.Zero);
            aRigidBody.SetAngularVelocity(0f);

            var dtc = new DeathTagComponent(a, default);
            a.Set(ref dtc);
        }
    }
}