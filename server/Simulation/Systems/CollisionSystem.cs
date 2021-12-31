using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public CollisionSystem() : base("Collision System", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => !ntt.IsBullet() && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity a, ref PhysicsComponent aPhy, ref ShapeComponent aShp, ref ViewportComponent aVwp)
        {
            if (aPhy.Position == aPhy.LastPosition)
                return;

            if (aPhy.Position.X < aShp.Radius)
            {
                aPhy.Velocity.X = Math.Abs(aPhy.Velocity.X);
                aPhy.Position.X = aShp.Radius;
            }
            else if (aPhy.Position.X > Game.MapSize.X - aShp.Radius)
            {
                aPhy.Velocity.X = -Math.Abs(aPhy.Velocity.X);
                aPhy.Position.X = Game.MapSize.X - aShp.Radius;
            }
            if (aPhy.Position.Y < aShp.Radius)
            {
                aPhy.Velocity.Y = Math.Abs(aPhy.Velocity.Y);
                aPhy.Position.Y = aShp.Radius;
            }
            else if (aPhy.Position.Y > Game.MapSize.Y - aShp.Radius)
            {
                aPhy.Velocity.Y = -Math.Abs(aPhy.Velocity.Y);
                aPhy.Position.Y = Game.MapSize.Y - aShp.Radius;
            }

            for (var k = 0; k < aVwp.EntitiesVisible.Count; k++)
            {
                ref readonly var b = ref aVwp.EntitiesVisible[k].Entity;

                if (b.Id == a.Id || b.IsBullet())
                    continue;
                if (!PixelWorld.EntityExists(in b))
                    continue;

                ref readonly var bShp = ref b.Get<ShapeComponent>();
                ref var bPhy = ref b.Get<PhysicsComponent>();

                if (!(aShp.Radius + bShp.Radius >= (bPhy.Position - aPhy.Position).Length()))
                    continue;

                var distance = aPhy.Position - bPhy.Position;
                var penetrationDepth = aShp.Radius + bShp.Radius - distance.Length();
                var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (aPhy.InverseMass + bPhy.InverseMass));
                aPhy.Position += penetrationResolution * aPhy.InverseMass;
                bPhy.Position += penetrationResolution * -bPhy.InverseMass;

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

                if (fa.X >= 0)
                    aPhy.AngularVelocity = fa.Length() / aShp.Radius;
                else
                    aPhy.AngularVelocity = -fa.Length() / aShp.Radius;

                if (fb.X >= 0)
                    bPhy.AngularVelocity = fb.Length() / bShp.Radius;
                else
                    bPhy.AngularVelocity = -fb.Length() / bShp.Radius;
            }
        }
    }
}