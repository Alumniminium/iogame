using System.Numerics;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Benchmarks
{
    public static class Bench
    {
        public static void Run()
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<CollisionBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class CollisionBenchmarks
    {
        [Benchmark]
        public void CheckCollisions()
        {
            foreach (var kvp in EntityManager.Entities)
            {
                var a = kvp.Value;
                var visible = CollisionDetection.Grid.GetEntitiesSameAndSurroundingCells(a);
                foreach (var b in visible)
                {
                    if (a.UniqueId == b.UniqueId)
                        return;
                    if (a is Bullet ba)
                    {
                        if (ba.Owner == b)
                            return;
                    }
                    if (b is Bullet bb)
                    {
                        if (bb.Owner == a)
                            return;
                    }
                    if (a is Bullet ab && b is Bullet bbb)
                    {
                        if (ab.Owner.UniqueId == bbb.Owner.UniqueId)
                            return;
                    }

                    if (a.IntersectsWith(b))
                    {
                        var aPos = a.PositionComponent.Position;
                        var bPos = b.PositionComponent.Position;

                        var (aVel, _, _) = a.VelocityComponent;
                        var (bVel, _, _) = b.VelocityComponent;

                        var dist = aPos - bPos;
                        var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.Magnitude();
                        var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
                        a.PositionComponent.Position += penRes * a.PhysicsComponent.InverseMass;
                        b.PositionComponent.Position += penRes * -b.PhysicsComponent.InverseMass;

                        var normal = (aPos - bPos).Unit();
                        var relVel = aVel - bVel;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
                        var impulseVec = normal * impulse;

                        if (a is Bullet bullet && b is not Bullet)
                        {
                            b.GetHitBy(a);
                            b.VelocityComponent.Movement += 10 * impulseVec * -b.PhysicsComponent.InverseMass;
                        }
                        else if (b is Bullet bullet2 && a is not Bullet)
                        {
                            a.GetHitBy(b);
                            a.VelocityComponent.Movement += 10 * impulseVec * a.PhysicsComponent.InverseMass;
                        }
                        else
                        {
                            a.VelocityComponent.Movement += impulseVec * a.PhysicsComponent.InverseMass;
                            b.VelocityComponent.Movement += impulseVec * -b.PhysicsComponent.InverseMass;
                        }

                        a.GetHitBy(b);
                        b.GetHitBy(a);
                    }
                }
            }
        }
        [Benchmark]
        public void CheckCollisionsParallelForEach()
        {
            Parallel.ForEach( EntityManager.Entities, kvp =>
            {
                var a = kvp.Value;
                var visible = CollisionDetection.Grid.GetEntitiesSameAndSurroundingCells(a);
                foreach (var b in visible)
                {
                    if (a.UniqueId == b.UniqueId)
                        return;
                    if (a is Bullet ba)
                    {
                        if (ba.Owner == b)
                            return;
                    }
                    if (b is Bullet bb)
                    {
                        if (bb.Owner == a)
                            return;
                    }
                    if (a is Bullet ab && b is Bullet bbb)
                    {
                        if (ab.Owner.UniqueId == bbb.Owner.UniqueId)
                            return;
                    }

                    if (a.IntersectsWith(b))
                    {
                        var aPos = a.PositionComponent.Position;
                        var bPos = b.PositionComponent.Position;

                        var (aVel, _, _) = a.VelocityComponent;
                        var (bVel, _, _) = b.VelocityComponent;

                        var dist = aPos - bPos;
                        var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.Magnitude();
                        var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
                        a.PositionComponent.Position += penRes * a.PhysicsComponent.InverseMass;
                        b.PositionComponent.Position += penRes * -b.PhysicsComponent.InverseMass;

                        var normal = (aPos - bPos).Unit();
                        var relVel = aVel - bVel;
                        var sepVel = Vector2.Dot(relVel, normal);
                        var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
                        var vsep_diff = new_sepVel - sepVel;

                        var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
                        var impulseVec = normal * impulse;

                        if (a is Bullet bullet && b is not Bullet)
                        {
                            b.GetHitBy(a);
                            b.VelocityComponent.Movement += 10 * impulseVec * -b.PhysicsComponent.InverseMass;
                        }
                        else if (b is Bullet bullet2 && a is not Bullet)
                        {
                            a.GetHitBy(b);
                            a.VelocityComponent.Movement += 10 * impulseVec * a.PhysicsComponent.InverseMass;
                        }
                        else
                        {
                            a.VelocityComponent.Movement += impulseVec * a.PhysicsComponent.InverseMass;
                            b.VelocityComponent.Movement += impulseVec * -b.PhysicsComponent.InverseMass;
                        }

                        a.GetHitBy(b);
                        b.GetHitBy(a);
                    }
                }
            });
        }
    }
}