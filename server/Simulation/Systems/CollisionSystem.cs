using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public CollisionSystem() : base("Collision System", Environment.ProcessorCount) { }
        public override void Update(float dt, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
                ref var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var shp = ref entity.Get<ShapeComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                if (pos.Position.X < shp.Radius)
                {
                    vel.Velocity.X = Math.Abs(vel.Velocity.X);
                    pos.Position.X = shp.Radius;
                }
                else if (pos.Position.X > Game.MapWidth - shp.Radius)
                {
                    vel.Velocity.X = -Math.Abs(vel.Velocity.X);
                    pos.Position.X = Game.MapWidth - shp.Radius;
                }
                if (pos.Position.Y < shp.Radius)
                {
                    vel.Velocity.Y = Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = shp.Radius;
                }
                else if (pos.Position.Y > Game.MapHeight - shp.Radius)
                {
                    vel.Velocity.Y = -Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = Game.MapHeight - shp.Radius;
                }

                for (int k = 0; k < vwp.EntitiesVisible.Length; k++)
                {
                    ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].EntityId);

                    if (other.EntityId == entity.EntityId)
                        continue;

                    if (!other.Has<PositionComponent, ShapeComponent, PhysicsComponent, VelocityComponent>())
                        continue;
                        
                    ref var otherPos = ref other.Get<PositionComponent>();
                    ref var otherShp = ref other.Get<ShapeComponent>();
                    ref var otherPhy = ref other.Get<PhysicsComponent>();
                    ref var otherVel = ref other.Get<VelocityComponent>();

                    if (shp.Radius + otherShp.Radius >= (otherPos.Position - pos.Position).Length())
                    {
                        if (!other.IsBullet())
                        {
                            var dist = pos.Position - otherPos.Position;
                            var penDepth = shp.Radius + otherShp.Radius - dist.Length();
                            var penRes = Vector2.Normalize(dist) * (penDepth / (phy.InverseMass + otherPhy.InverseMass));
                            pos.Position += penRes * phy.InverseMass;
                            otherPos.Position += penRes * -otherPhy.InverseMass;
                        }

                        var normal = Vector2.Normalize(pos.Position - otherPos.Position);
                        var relVel = vel.Velocity - otherVel.Velocity;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var newSepVel = -sepVel * Math.Min(phy.Elasticity, otherPhy.Elasticity);
                        var vsepDiff = newSepVel - sepVel;

                        var impulse = vsepDiff / (phy.InverseMass + otherPhy.InverseMass);
                        var impulseVec = normal * impulse;

                        var fa = impulseVec * phy.InverseMass;
                        var fb = impulseVec * -otherPhy.InverseMass;


                        if (entity.IsBullet())
                        {
                            otherVel.Velocity += fb * dt;
                            vel.Velocity *= 0.99f;
                        }
                        else
                            vel.Velocity += fa;

                        if (other.IsBullet())
                        {
                            vel.Velocity += fa * dt;
                            otherVel.Velocity *= 0.99f;
                        }
                        else
                            otherVel.Velocity += fb;
                    }
                }
            }
        }
    }
}