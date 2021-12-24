using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PositionComponent, PhysicsComponent, ShapeComponent>
    {
        public CollisionSystem() : base("Collision System", threads: Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var a = entities[i];
                ref var pos = ref a.Get<PositionComponent>();

                if (pos.Position == pos.LastPosition)
                    continue;

                ref readonly var aShp = ref a.Get<ShapeComponent>();
                ref readonly var aVwp = ref a.Get<ViewportComponent>();
                ref var aPhy = ref a.Get<PhysicsComponent>();

                if (pos.Position.X < aShp.Radius)
                {
                    aPhy.Velocity.X = Math.Abs(aPhy.Velocity.X);
                    pos.Position.X = aShp.Radius;
                }
                else if (pos.Position.X > Game.MapSize.X - aShp.Radius)
                {
                    aPhy.Velocity.X = -Math.Abs(aPhy.Velocity.X);
                    pos.Position.X = Game.MapSize.X - aShp.Radius;
                }
                if (pos.Position.Y < aShp.Radius)
                {
                    aPhy.Velocity.Y = Math.Abs(aPhy.Velocity.Y);
                    pos.Position.Y = aShp.Radius;
                }
                else if (pos.Position.Y > Game.MapSize.Y - aShp.Radius)
                {
                    aPhy.Velocity.Y = -Math.Abs(aPhy.Velocity.Y);
                    pos.Position.Y = Game.MapSize.Y - aShp.Radius;
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
                    ref var bPos = ref b.Get<PositionComponent>();

                    if (!(aShp.Radius + bShp.Radius >= (bPos.Position - pos.Position).Length()))
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

                    ref var bPhy = ref b.Get<PhysicsComponent>();

                    var distance = pos.Position - bPos.Position;
                    var penetrationDepth = aShp.Radius + bShp.Radius - distance.Length();
                    var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (aPhy.InverseMass + bPhy.InverseMass));
                    pos.Position += penetrationResolution * aPhy.InverseMass;
                    bPos.Position += penetrationResolution * -bPhy.InverseMass;

                    var normal = Vector2.Normalize(pos.Position - bPos.Position);
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