using System.Numerics;
using iogame.ECS;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class CollisionSystem : PixelSystem<PositionComponent, VelocityComponent, PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public CollisionSystem() : base(4)
        {
            Name = "Collision System";
            PerformanceMetrics.RegisterSystem(Name);
        }

        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref var phy = ref entity.Get<PhysicsComponent>();
                ref var pos = ref entity.Get<PositionComponent>();
                ref var vel = ref entity.Get<VelocityComponent>();
                ref var shp = ref entity.Get<ShapeComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                for (int k = 0; k < vwp.EntitiesVisible.Count; k++)
                {
                    var other = vwp.EntitiesVisible[k];

                    if (other.EntityId == entity.EntityId)
                        continue;

                    ref var otherPos = ref other.Entity.Get<PositionComponent>();
                    ref var otherShp = ref other.Entity.Get<ShapeComponent>();
                    ref var otherPhy = ref other.Entity.Get<PhysicsComponent>();
                    ref var otherVel = ref other.Entity.Get<VelocityComponent>();

                    if (shp.Radius + otherShp.Radius >= (otherPos.Position - pos.Position).Magnitude())
                    {
                        // if (other is not Bullet)
                        // {
                        var dist = pos.Position - otherPos.Position;
                        var penDepth = shp.Radius + otherShp.Radius - dist.Length();
                        var penRes = dist.Unit() * (penDepth / (phy.InverseMass + otherPhy.InverseMass));
                        pos.Position += penRes * phy.InverseMass;
                        otherPos.Position += penRes * -otherPhy.InverseMass;
                        // }

                        var normal = (pos.Position - otherPos.Position).Unit();
                        var relVel = vel.Velocity - otherVel.Velocity;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(phy.Elasticity, otherPhy.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (phy.InverseMass + otherPhy.InverseMass);
                        var impulseVec = normal * impulse;

                        var fa = impulseVec * phy.InverseMass;
                        var fb = impulseVec * -otherPhy.InverseMass;


                        // if (entity.EntityId < 10_000_000 || entity.EntityId > 100_000_000)
                        // {
                        otherVel.Velocity += fb;
                        vel.Velocity *= 0.99f;
                        // }
                        // else
                        // vel.Velocity += fa;

                        // if (other is not Bullet)
                        // {
                        vel.Velocity += fa;
                        otherVel.Velocity *= 0.99f;
                        // }
                        // else
                        // otherVel.Velocity += fb;
                    }
                }
                if (pos.Position.X < shp.Radius)
                {
                    vel.Velocity.X = Math.Abs(vel.Velocity.X);
                    pos.Position.X = shp.Radius;
                }
                else if (pos.Position.X > Game.MAP_WIDTH - shp.Radius)
                {
                    vel.Velocity.X = -Math.Abs(vel.Velocity.X);
                    pos.Position.X = Game.MAP_WIDTH - shp.Radius;
                }
                if (pos.Position.Y < shp.Radius)
                {
                    vel.Velocity.Y = Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = shp.Radius;
                }
                else if (pos.Position.Y > Game.MAP_HEIGHT - shp.Radius)
                {
                    vel.Velocity.Y = -Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = Game.MAP_HEIGHT - shp.Radius;
                }
            }
        }
    }
}