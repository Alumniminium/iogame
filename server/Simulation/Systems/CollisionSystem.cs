using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public CollisionSystem() : base("Collision System", threads: Environment.ProcessorCount) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var a = ref entities[i];
                ref var aPhy = ref a.Get<PhysicsComponent>();

                if (aPhy.Position == aPhy.LastPosition)
                    continue;

                ref readonly var aShp = ref a.Get<ShapeComponent>();
                ref readonly var aVwp = ref a.Get<ViewportComponent>();

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

                for (var k = 0; k < aVwp.ChangedEntities.Count; k++)
                {
                    var visible = aVwp.ChangedEntities[k];
                    ref readonly var b = ref visible.Entity;

                    if (b.Id == a.Id)
                        continue;
                    if (!PixelWorld.EntityExists(in b))
                        continue;

                    ref readonly var bShp = ref b.Get<ShapeComponent>();
                    ref var bPhy = ref b.Get<PhysicsComponent>();

                    if (!(aShp.Radius + bShp.Radius >= (bPhy.Position - aPhy.Position).Length()))
                        continue;


                    if (a.IsBullet())
                    {
                        ref readonly var ab = ref a.Get<BulletComponent>();

                        if (ab.Owner.Id == b.Id)
                            continue;

                        if (b.IsBullet())
                        {
                            ref readonly var bb = ref b.Get<BulletComponent>();

                            if (bb.Owner.Id == a.Id || bb.Owner.Id == ab.Owner.Id)
                                continue;
                        }
                    }

                    if (b.IsBullet())
                    {
                        ref readonly var bb = ref b.Get<BulletComponent>();

                        if (bb.Owner.Id == a.Id)
                            continue;
                    }

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

                    if (a.IsBullet())
                        aPhy.Velocity *= 0.9f;
                    else
                        aPhy.Velocity += fa;

                    if (b.IsBullet())
                        bPhy.Velocity *= 0.9f;
                    else
                        bPhy.Velocity += fb;
                }
            }
        }
    }
}