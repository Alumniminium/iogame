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

        protected override bool MatchesFilter(in PixelEntity entity) => entity.IsBullet() ? false : base.MatchesFilter(entity);

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref var pos = ref entity.Get<PositionComponent>();

                if (pos.Position == pos.LastPosition)
                    continue;

                var shpEntity = PixelWorld.GetAttachedShapeEntity(in entity);
                ref var vel = ref entity.Get<VelocityComponent>();
                ref readonly var shp = ref entity.Get<ShapeComponent>();
                ref readonly var phy = ref entity.Get<PhysicsComponent>();

                if (pos.Position.X < shp.Radius)
                {
                    vel.Velocity.X = Math.Abs(vel.Velocity.X);
                    pos.Position.X = shp.Radius;
                }
                else if (pos.Position.X > Game.MapSize.X - shp.Radius)
                {
                    vel.Velocity.X = -Math.Abs(vel.Velocity.X);
                    pos.Position.X = Game.MapSize.X - shp.Radius;
                }
                if (pos.Position.Y < shp.Radius)
                {
                    vel.Velocity.Y = Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = shp.Radius;
                }
                else if (pos.Position.Y > Game.MapSize.Y - shp.Radius)
                {
                    vel.Velocity.Y = -Math.Abs(vel.Velocity.Y);
                    pos.Position.Y = Game.MapSize.Y - shp.Radius;
                }
                var rect = shpEntity.Rect;
                var visible = Pool<List<ShapeEntity>>.Shared.Get();
                Game.Tree.GetObjects(rect, visible);

                for (var k = 0; k < visible.Count; k++)
                {
                    ref var other = ref PixelWorld.GetEntity(visible[k].Entity.EntityId);

                    if (other.EntityId == entity.EntityId || other.IsBullet())
                        continue;

                    ref readonly var otherShp = ref other.Get<ShapeComponent>();
                    ref readonly var otherPhy = ref other.Get<PhysicsComponent>();
                    ref var otherPos = ref other.Get<PositionComponent>();
                    ref var otherVel = ref other.Get<VelocityComponent>();

                    if (shp.Radius + otherShp.Radius >= (otherPos.Position - pos.Position).Length())
                    {
                        var distance = pos.Position - otherPos.Position;
                        var penetrationDepth = shp.Radius + otherShp.Radius - distance.Length();
                        var penetrationResolution = Vector2.Normalize(distance) * (penetrationDepth / (phy.InverseMass + otherPhy.InverseMass));
                        pos.Position += penetrationResolution * phy.InverseMass;
                        otherPos.Position += penetrationResolution * -otherPhy.InverseMass;

                        var collisionDirection = Vector2.Normalize(pos.Position - otherPos.Position);
                        var collisionVelocity = vel.Velocity - otherVel.Velocity;
                        var seperationVelocity = Vector2.Dot(collisionVelocity, collisionDirection);
                        var newSeperationVelocity = -seperationVelocity * Math.Min(phy.Elasticity, otherPhy.Elasticity);
                        var seperationDelta = newSeperationVelocity - seperationVelocity;

                        var impulse = seperationDelta / (phy.InverseMass + otherPhy.InverseMass) * collisionDirection;

                        var forceEntity = impulse * phy.InverseMass;
                        var forceOther = impulse * -otherPhy.InverseMass;


                        vel.Velocity += forceEntity;
                        var dmg = new DamageComponent(1);
                        other.Add(in dmg);

                        otherVel.Velocity += forceOther;
                        entity.Add(in dmg);
                    }
                }
                visible.Clear();
                Pool<List<ShapeEntity>>.Shared.Return(visible);
            }
        }
    }
}