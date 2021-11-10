using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation
{
    public static class CollisionDetection
    {
        public static readonly Grid Grid = new(Game.MAP_WIDTH, Game.MAP_HEIGHT, 300, 300);
        static CollisionDetection() => PerformanceMetrics.RegisterSystem(nameof(CollisionDetection));

        public static unsafe void Process(float dt)
        {
            Grid.Clear();
            foreach (var kvp in World.ShapeEntities)
            {
                var a = kvp.Value;
                Grid.Insert(a);
            }
            
            foreach (var kvp in World.ShapeEntities)
            {
                var a = kvp.Value;
                var visible = Grid.GetEntitiesSameAndSurroundingCellsList(a);
                foreach (var b in visible)
                {
                    if (!ValidPair(a, b))
                        continue;
                    if (a.IntersectsWith(b))
                    {
                        ResolveCollision(a, b, dt);
                        ApplyDamage(a, b);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool ValidPair(ShapeEntity a, ShapeEntity b)
        {
            if (!World.EntityExists(a.EntityId))
                return false;
            if (!World.EntityExists(a.EntityId))
                return false;
            if (a.EntityId == b.EntityId)
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
                if (ab.Owner.EntityId == bbb.Owner.EntityId)
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void ResolveCollision(ShapeEntity a, ShapeEntity b, float dt)
        {
            if (a is not Bullet && b is not Bullet)
                ResolvePenetration(a, b);

            if (!a.Entity.Has<VelocityComponent>())
                a.Entity.Add<VelocityComponent>();
            if (!b.Entity.Has<VelocityComponent>())
                b.Entity.Add<VelocityComponent>();

            ref var aPos = ref a.PositionComponent.Position;
            ref var bPos = ref b.PositionComponent.Position;
            ref var aVel = ref a.VelocityComponent.Movement;
            ref var bVel = ref b.VelocityComponent.Movement;

            var normal = (aPos - bPos).Unit();
            var relVel = aVel - bVel;
            var sepVel = Vector2.Dot(relVel, normal);
            var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
            var vsep_diff = new_sepVel - sepVel;

            var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
            var impulseVec = normal * impulse;

            var fa = impulseVec * a.PhysicsComponent.InverseMass;
            var fb = impulseVec * -b.PhysicsComponent.InverseMass;


            if (a is Bullet)
            {
                bVel += fb * dt;
                aVel *= 1 - 0.99f * dt;
            }
            else
                aVel += fa;

            if (b is Bullet)
            {
                aVel += fa*dt;
                bVel *= 1 - 0.99f * dt;
            }
            else
                bVel += fb;
        }

        private static void ResolvePenetration(ShapeEntity a, ShapeEntity b)
        {
            ref var aPos = ref a.PositionComponent.Position;
            ref var bPos = ref b.PositionComponent.Position;

            var dist = aPos - bPos;
            var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.Magnitude();
            var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
            aPos += penRes * a.PhysicsComponent.InverseMass;
            bPos += penRes * -b.PhysicsComponent.InverseMass;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void ApplyDamage(ShapeEntity a, ShapeEntity b)
        {
            if (a is Bullet && b is not Bullet)
                b.GetHitBy(a);
            else if (b is Bullet && a is not Bullet)
                a.GetHitBy(b);
        }
    }
}