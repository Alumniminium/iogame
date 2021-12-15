using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class BulletCollisionSystem : PixelSystem<BulletComponent>
    {
        public BulletCollisionSystem() : base("Bullet Collision System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var a = entities[i];
                ref readonly var aShp = ref a.Get<ShapeComponent>();
                ref readonly var aBul = ref a.Get<BulletComponent>();
                ref readonly var aPhy = ref a.Get<PhysicsComponent>();

                ref var aPos = ref a.Get<PositionComponent>();
                ref var aVel = ref a.Get<VelocityComponent>();
                var aShpEntity = PixelWorld.GetAttachedShapeEntity(in a);
                var rect = aShpEntity.Rect;

                var visible = Pool<List<ShapeEntity>>.Shared.Get();
                Game.Tree.GetObjects(rect, visible);

                for (var k = 0; k < visible.Count; k++)
                {
                    ref var b = ref visible[k].Entity;
                    ref readonly var bShp = ref b.Get<ShapeComponent>();
                    ref readonly var bPhy = ref b.Get<PhysicsComponent>();
                    ref var bPos = ref b.Get<PositionComponent>();
                    ref var bVel = ref b.Get<VelocityComponent>();

                    var collisionDirection = Vector2.Normalize(aPos.Position - bPos.Position);
                    var collisionVelocity = aVel.Velocity - bVel.Velocity;
                    var seperationVelocity = Vector2.Dot(collisionVelocity, collisionDirection);
                    var newSeperationVelocity = -seperationVelocity * Math.Min(aPhy.Elasticity, bPhy.Elasticity);
                    var seperationDelta = newSeperationVelocity - seperationVelocity;
                    var impulse = seperationDelta / (aPhy.InverseMass + bPhy.InverseMass) * collisionDirection;

                    if (b.IsBullet())
                    {
                        ref var bb = ref b.Get<BulletComponent>();

                        if (aBul.Owner.EntityId == bb.Owner.EntityId)
                            continue;
                        if (aBul.Owner.EntityId == b.EntityId)
                            continue;
                        if (bb.Owner.EntityId == a.EntityId)
                            continue;

                        bVel.Velocity *= 0.9f;
                    }
                    else
                    {
                        bVel.Velocity += impulse * -bPhy.InverseMass * dt;
                    }

                    aVel.Velocity *= 0.9f;

                    var dmgB = new DamageComponent((impulse * -bPhy.InverseMass).Length() / 100);
                    var dmgA = new DamageComponent((impulse * aPhy.InverseMass).Length() / 100);
                    b.Add(in dmgB);
                    a.Add(in dmgA);
                }
                Pool<List<ShapeEntity>>.Shared.Return(visible);
            }
        }
    }
}