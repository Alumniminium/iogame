using System.Diagnostics;
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
                aPhy.Velocity.X = MathF.Abs(aPhy.Velocity.X);
                aPhy.Position.X = aShp.Radius;
            }
            else if (aPhy.Position.X > Game.MapSize.X - aShp.Radius)
            {
                aPhy.Velocity.X = -MathF.Abs(aPhy.Velocity.X);
                aPhy.Position.X = Game.MapSize.X - aShp.Radius;
            }
            if (aPhy.Position.Y < aShp.Radius)
            {
                aPhy.Velocity.Y = MathF.Abs(aPhy.Velocity.Y);
                aPhy.Position.Y = aShp.Radius;
            }
            else if (aPhy.Position.Y > Game.MapSize.Y - aShp.Radius)
            {
                aPhy.Velocity.Y = -MathF.Abs(aPhy.Velocity.Y);
                aPhy.Position.Y = Game.MapSize.Y - aShp.Radius;
            }

            if (a.IsFood())
            {
                aVwp.EntitiesVisible.Clear();
                Game.Tree.GetObjects(aVwp.Viewport, aVwp.EntitiesVisible);
            }
            for (var k = 0; k < aVwp.EntitiesVisible.Count; k++)
            {
                if (aVwp.EntitiesVisible[k] == null)
                {
                    aVwp.EntitiesVisible.RemoveAt(k);
                    k--;
                    continue;
                }

                ref readonly var b = ref aVwp.EntitiesVisible[k].Entity;

                if (b.Id == a.Id || b.IsBullet())
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
                var newSepVel = -sepVel * MathF.Min(aPhy.Elasticity, bPhy.Elasticity);
                var vsepDiff = newSepVel - sepVel;

                var impulse = vsepDiff / (aPhy.InverseMass + bPhy.InverseMass);
                var impulseVec = normal * impulse;

                var fa = impulseVec * aPhy.InverseMass;
                var fb = impulseVec * -bPhy.InverseMass;

                aPhy.Velocity += fa;
                bPhy.Velocity += fb;

                var afa = fa.X >= 0 ? fa.Length() / aShp.Radius : -(fa.Length() / aShp.Radius);
                var afb = fb.X >= 0 ? fb.Length() / bShp.Radius : -(fb.Length() / bShp.Radius);
                aPhy.AngularVelocity += afa;
                bPhy.AngularVelocity += afb;
            }
        }
    }
}