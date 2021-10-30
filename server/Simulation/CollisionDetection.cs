using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Simulation
{
    public static class CollisionDetection
    {
        public static readonly Grid Grid = new(Game.MAP_WIDTH, Game.MAP_HEIGHT, 500, 500);
        static CollisionDetection() => PerformanceMetrics.RegisterSystem(nameof(CollisionDetection));
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void Process(float dt)
        {
            foreach (var kvp in EntityManager.Entities)
            {
                var a = kvp.Value;
                var visible = Grid.GetEntitiesSameCell(a);
                foreach (var b in visible)
                {
                    if (a.IntersectsWith(b))
                    {
                        if (!ValidPair(a, b))
                            continue;
                        ResolveCollision(a, b, dt);
                        ApplyDamage(a, b);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool ValidPair(Entity a, Entity b)
        {
            if (a.UniqueId == b.UniqueId)
                return false;

            if (a is Bullet ba)
            {
                if (ba.Owner == b)
                    return false;
            }
            if (b is Bullet bb)
            {
                if (bb.Owner == a)
                    return false;
            }

            if (a is Bullet ab && b is Bullet bbb)
            {
                if (ab.Owner.UniqueId == bbb.Owner.UniqueId)
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void ResolveCollision(Entity a, Entity b, float dt)
        {
            if (a is not Bullet && b is not Bullet)
                ResolvePenetration(a, b);

            var aPos = a.PositionComponent.Position;
            var bPos = b.PositionComponent.Position;
            var (aVel, _, _) = a.VelocityComponent;
            var (bVel, _, _) = b.VelocityComponent;

            var normal = (aPos - bPos).Unit();
            var relVel = aVel - bVel;
            var sepVel = Vector2.Dot(relVel, normal);
            var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
            var vsep_diff = new_sepVel - sepVel;

            var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
            var impulseVec = normal * impulse;


            if (a is Bullet)
            {
                b.VelocityComponent.Movement += 0.5f * impulseVec * b.PhysicsComponent.InverseMass;
                a.VelocityComponent.Movement *= 1 - 0.99f * dt;
            }
            else
                a.VelocityComponent.Movement += impulseVec * a.PhysicsComponent.InverseMass;

            if (b is Bullet)
            {
                a.VelocityComponent.Movement += 0.5f * impulseVec * a.PhysicsComponent.InverseMass;
                b.VelocityComponent.Movement *= 1 - 0.99f * dt;
            }
            else
                b.VelocityComponent.Movement += impulseVec * -b.PhysicsComponent.InverseMass;
        }

        private static void ResolvePenetration(Entity a, Entity b)
        {
            var aPos = a.PositionComponent.Position;
            var bPos = b.PositionComponent.Position;

            var dist = aPos - bPos;
            var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.Magnitude();
            var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
            a.PositionComponent.Position += penRes * a.PhysicsComponent.InverseMass;
            b.PositionComponent.Position += penRes * -b.PhysicsComponent.InverseMass;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void ApplyDamage(Entity a, Entity b)
        {
            if (a is Bullet && b is not Bullet)
                b.GetHitBy(a);
            else if (b is Bullet && a is not Bullet)
                a.GetHitBy(b);
        }
    }
}