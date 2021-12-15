using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent, ShapeComponent>
    {
        public CollisionSystem() : base("Collision System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var a = entities[i];
                ref var pos = ref a.Get<PositionComponent>();

                if (pos.Position == pos.LastPosition)
                    continue;

                // var shpEntity = PixelWorld.GetAttachedShapeEntity(in a);
                ref var aVel = ref a.Get<VelocityComponent>();
                ref readonly var aShp = ref a.Get<ShapeComponent>();
                ref readonly var aPhy = ref a.Get<PhysicsComponent>();
                ref readonly var aVwp = ref a.Get<ViewportComponent>();

                // if (pos.Position.X < aShp.Radius)
                // {
                //     aVel.Velocity.X = Math.Abs(aVel.Velocity.X);
                //     pos.Position.X = aShp.Radius;
                // }
                // else if (pos.Position.X > Game.MapSize.X - aShp.Radius)
                // {
                //     aVel.Velocity.X = -Math.Abs(aVel.Velocity.X);
                //     pos.Position.X = Game.MapSize.X - aShp.Radius;
                // }
                // if (pos.Position.Y < aShp.Radius)
                // {
                //     aVel.Velocity.Y = Math.Abs(aVel.Velocity.Y);
                //     pos.Position.Y = aShp.Radius;
                // }
                // else if (pos.Position.Y > Game.MapSize.Y - aShp.Radius)
                // {
                //     aVel.Velocity.Y = -Math.Abs(aVel.Velocity.Y);
                //     pos.Position.Y = Game.MapSize.Y - aShp.Radius;
                // }

                for (var k = 0; k < aVwp.EntitiesVisible.Count; k++)
                {
                    ref readonly var b = ref aVwp.EntitiesVisible[k].Entity;
                    if (!PixelWorld.EntityExists(in b))
                        continue;

                    if (b.EntityId == a.EntityId || b.IsBullet())
                        continue;

                    ref readonly var bShp = ref b.Get<ShapeComponent>();
                    ref readonly var bPhy = ref b.Get<PhysicsComponent>();
                    ref var bPos = ref b.Get<PositionComponent>();
                    ref var bVel = ref b.Get<VelocityComponent>();

                    if (aShp.Radius + bShp.Radius >= (bPos.Position - pos.Position).Length())
                    {
                        var distance = pos.Position - bPos.Position;
                        var penetrationDepth = aShp.Radius + bShp.Radius - distance.Length();
                        var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (aPhy.InverseMass + bPhy.InverseMass));
                        pos.Position += penetrationResolution * aPhy.InverseMass;
                        bPos.Position += penetrationResolution * -bPhy.InverseMass;

                        var collisionDirection = Vector2.Normalize(pos.Position - bPos.Position);
                        var collisionVelocity = aVel.Velocity - bVel.Velocity;
                        var seperationVelocity = Vector2.Dot(collisionVelocity, collisionDirection);
                        var newSeperationVelocity = -seperationVelocity * Math.Min(aPhy.Elasticity, bPhy.Elasticity);
                        var seperationDelta = newSeperationVelocity - seperationVelocity;

                        var impulse = seperationDelta / (aPhy.InverseMass + bPhy.InverseMass) * collisionDirection;

                        var forceEntity = impulse * aPhy.InverseMass;
                        var forceOther = impulse * -bPhy.InverseMass;


                        if (a.IsBullet())
                        {
                            ref readonly var aBul = ref a.Get<BulletComponent>();
                            if (b.IsBullet())
                            {
                                ref readonly var bb = ref b.Get<BulletComponent>();

                                if (aBul.Owner.EntityId == bb.Owner.EntityId)
                                    continue;
                                if (aBul.Owner.EntityId == b.EntityId)
                                    continue;
                                if (bb.Owner.EntityId == a.EntityId)
                                    continue;

                                bVel.Velocity *= 0.98f;
                            }
                            else
                            {
                                bVel.Velocity += impulse * -bPhy.InverseMass * dt;
                            }

                            var dmgB = new DamageComponent((impulse * -bPhy.InverseMass).Length() / 100);
                            var dmgA = new DamageComponent((impulse * aPhy.InverseMass).Length() / 100);
                            b.Add(ref dmgB);
                            a.Add(ref dmgA);
                        }
                        else
                        {
                            aVel.Velocity += forceEntity;
                            var dmg = new DamageComponent(1);
                            b.Add(ref dmg);

                            bVel.Velocity += forceOther;
                            a.Add(ref dmg);
                        }
                    }
                }
            }
        }
    }
}