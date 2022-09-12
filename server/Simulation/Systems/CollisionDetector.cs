using System;
using System.Collections.Generic;
using System.Drawing;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class CollisionDetector : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: Environment.ProcessorCount) { }
        public override void Update(in PixelEntity a, ref PhysicsComponent aPhy, ref ShapeComponent aShp)
        {
            if (Game.CurrentTick % 2 != 0)
                return;

            if (aPhy.Position == aPhy.LastPosition || a.Has<CollisionComponent>())
                return;

            var collsisions = Pool<List<ShapeEntity>>.Shared.Get();
            Game.Tree.GetObjects(new RectangleF(aPhy.Position.X - aShp.Radius, aPhy.Position.Y - aShp.Radius, aShp.Size, aShp.Size), collsisions);

            for (var k = 0; k < collsisions.Count; k++)
            {
                ref readonly var b = ref collsisions[k].Entity;

                if (b.Id == a.Id || b.Has<CollisionComponent>())
                    continue;

                ref var bPhy = ref b.Get<PhysicsComponent>();
                bool collided = false;

                if (b.Has<ShapeComponent>())
                {
                    ref readonly var bShp = ref b.Get<ShapeComponent>();

                    if (!(aShp.Radius + bShp.Radius >= (bPhy.Position - aPhy.Position).Length()))
                        continue;

                    collided = true;
                }
                else
                {
                    ref readonly var bPoly = ref b.Get<PolygonComponent>();

                    // do collision check

                    // collided = true;
                }

                if (collided)
                {
                    var col = new CollisionComponent(a, b);
                    a.Add(ref col);
                    b.Add(ref col);
                }
            }
            collsisions.Clear();
            Pool<List<ShapeEntity>>.Shared.Return(collsisions);
        }
    }
}