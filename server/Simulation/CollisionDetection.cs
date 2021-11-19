using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation
{
    public static class CollisionDetection // todo quadtree
    {
        private static readonly Stopwatch sw = Stopwatch.StartNew();
        public static readonly Grid Grid = new(Game.MAP_WIDTH, Game.MAP_HEIGHT, 25, 25);
        static CollisionDetection() => PerformanceMetrics.RegisterSystem(nameof(CollisionDetection));

        public static unsafe void Process(float dt)
        {
            var last = sw.Elapsed.TotalMilliseconds;
            Grid.Clear();
            PerformanceMetrics.AddSample("Grid.Clear", sw.Elapsed.TotalMilliseconds - last);
            last = sw.Elapsed.TotalMilliseconds;
            foreach (var kvp in World.ShapeEntities)
            {
                var a = kvp.Value;
                Grid.Insert(a);
            }
            PerformanceMetrics.AddSample("Grid.Insert", sw.Elapsed.TotalMilliseconds - last);

            last = sw.Elapsed.TotalMilliseconds;
            foreach (var kvp in World.ShapeEntities)
            {
                var a = kvp.Value;

                ResolveEdgeCollision(a);
                var visible = Grid.GetEntitiesSameAndSurroundingCells(a);
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
            PerformanceMetrics.AddSample(nameof(CollisionDetection), sw.Elapsed.TotalMilliseconds - last);
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

            ref var aPos = ref a.PositionComponent.Position;
            ref var bPos = ref b.PositionComponent.Position;
            ref var aVel = ref a.VelocityComponent.Velocity;
            ref var bVel = ref b.VelocityComponent.Velocity;

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
                bVel += fb;
                aVel *= 0.99f;
            }
            else
                aVel += fa;

            if (b is Bullet)
            {
                aVel += fa;
                bVel *= 0.99f;
            }
            else
                bVel += fb;
        }
        public static void ResolveEdgeCollision(ShapeEntity a)
        {
            ref var pos = ref a.PositionComponent.Position;
            ref var vel = ref a.VelocityComponent.Velocity;
            ref var shp = ref a.ShapeComponent.Size;

                // Check for left and right
                if (pos.X < shp){
                    vel.X = Math.Abs(vel.X);
                    pos.X = shp;
                }
                else if (pos.X > Game.MAP_WIDTH - shp){
                    vel.X = -Math.Abs(vel.X);
                    pos.X = Game.MAP_WIDTH  - shp;
                }

                // Check for bottom and top
                if (pos.Y < shp){
                    vel.Y = Math.Abs(vel.Y);
                    pos.Y = shp;
                } else if (pos.Y > Game.MAP_HEIGHT - shp){
                    vel.Y = -Math.Abs(vel.Y);
                    pos.Y = Game.MAP_HEIGHT - shp;
                }
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
            b.GetHitBy(a);
            a.GetHitBy(b);
        }
    }
}