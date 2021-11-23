using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;
using QuadTrees;

namespace iogame.Simulation
{
    public static class CollisionDetection
    {
        // private static readonly Stopwatch sw = Stopwatch.StartNew();
        // public static readonly Grid Grid = new(Game.MAP_WIDTH, Game.MAP_HEIGHT, 20, 20);
        public static readonly QuadTreeRectF<ShapeEntity> Tree = new(0,0,Game.MAP_WIDTH,Game.MAP_HEIGHT);
        // static CollisionDetection() => PerformanceMetrics.RegisterSystem(nameof(CollisionDetection));

        // public static unsafe void Process(float dt)
        // {
        //     var last = sw.Elapsed.TotalMilliseconds;
        //     PixelWorld.ShapeEntities.AsParallel().ForAll((kvp) => 
        //     {
        //         var a = kvp.Value;

        //         ResolveEdgeCollision(a);
        //         var visible = Tree.GetObjects(a.Rect);
        //         foreach (var b in visible)
        //         {
        //             if (!ValidPair(a, b))
        //                 continue;

        //             if (a.IntersectsWith(b))
        //             {
        //                 ResolveCollision(a, b, dt);
        //                 ApplyDamage(a, b);
        //             }
        //         }
        //     });
        //     PerformanceMetrics.AddSample(nameof(CollisionDetection), sw.Elapsed.TotalMilliseconds - last);
        // }

        // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // private static bool ValidPair(ShapeEntity a, ShapeEntity b)
        // {
        //     if (a.EntityId == b.EntityId)
        //         return false;

        //     if (a.Owner == b || b.Owner == a)
        //         return false;

        //     if (a.Owner != null && b.Owner != null && a.Owner == b.Owner)
        //         return false;

        //     return true;
        // }

        // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // private static void ResolveCollision(ShapeEntity a, ShapeEntity b, float dt)
        // {
        //     if (a is not Bullet && b is not Bullet)
        //         ResolvePenetration(a, b);

        //     ref var aPos = ref a.PositionComponent.Position;
        //     ref var bPos = ref b.PositionComponent.Position;
        //     ref var aVel = ref a.VelocityComponent.Velocity;
        //     ref var bVel = ref b.VelocityComponent.Velocity;

        //     var normal = Vector2.Normalize(aPos - bPos);
        //     var relVel = aVel - bVel;
        //     var sepVel = Vector2.Dot(relVel, normal);
        //     var new_sepVel = -sepVel * Math.Min(a.PhysicsComponent.Elasticity, b.PhysicsComponent.Elasticity);
        //     var vsep_diff = new_sepVel - sepVel;

        //     var impulse = vsep_diff / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass);
        //     var impulseVec = normal * impulse;

        //     var fa = impulseVec * a.PhysicsComponent.InverseMass;
        //     var fb = impulseVec * -b.PhysicsComponent.InverseMass;


        //     if (a is Bullet)
        //     {
        //         bVel += fb;
        //         aVel *= 0.99f;
        //     }
        //     else
        //         aVel += fa;

        //     if (b is Bullet)
        //     {
        //         aVel += fa;
        //         bVel *= 0.99f;
        //     }
        //     else
        //         bVel += fb;
        // }
        // public static void ResolveEdgeCollision(ShapeEntity a)
        // {
        //     ref var pos = ref a.PositionComponent.Position;
        //     ref var vel = ref a.VelocityComponent.Velocity;
        //     ref readonly var shp = ref a.ShapeComponent.Size;

        //         // Check for left and right
        //         if (pos.X < shp){
        //             vel.X = Math.Abs(vel.X);
        //             pos.X = shp;
        //         }
        //         else if (pos.X > Game.MAP_WIDTH - shp){
        //             vel.X = -Math.Abs(vel.X);
        //             pos.X = Game.MAP_WIDTH  - shp;
        //         }
        //         // Check for bottom and top
        //         if (pos.Y < shp){
        //             vel.Y = Math.Abs(vel.Y);
        //             pos.Y = shp;
        //         } else if (pos.Y > Game.MAP_HEIGHT - shp){
        //             vel.Y = -Math.Abs(vel.Y);
        //             pos.Y = Game.MAP_HEIGHT - shp;
        //         }
        // }
        // private static void ResolvePenetration(ShapeEntity a, ShapeEntity b)
        // {
        //     ref var aPos = ref a.PositionComponent.Position;
        //     ref var bPos = ref b.PositionComponent.Position;

        //     var dist = aPos - bPos;
        //     var penDepth = a.ShapeComponent.Radius + b.ShapeComponent.Radius * a.ShapeComponent.Radius + b.ShapeComponent.Radius - dist.LengthSquared();
        //     var penRes = dist.Unit() * (penDepth / (a.PhysicsComponent.InverseMass + b.PhysicsComponent.InverseMass));
        //     aPos += penRes * a.PhysicsComponent.InverseMass;
        //     bPos += penRes * -b.PhysicsComponent.InverseMass;
        // }

        // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // private static void ApplyDamage(ShapeEntity a, ShapeEntity b)
        // {
        //     b.GetHitBy(a);
        //     a.GetHitBy(b);
        // }
    }
}