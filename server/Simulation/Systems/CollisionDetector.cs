using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionDetector : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: Environment.ProcessorCount) { }
        public override void Update(in PixelEntity ntt, ref PhysicsComponent c1, ref ShapeComponent c2)
        {
            if (c1.Position == c1.LastPosition || ntt.Has<CollisionComponent>())
                return;

            var collsisions = Game.Grid.GetEntitiesSameAndSurroundingCells(ntt);

            for (var k = 0; k < collsisions.Count; k++)
            {
                var b = collsisions[k];

                if (b.Id == ntt.Id)
                    continue;

                ref var bPhy = ref b.Get<PhysicsComponent>();
                bool collided = false;

                if (b.Has<ShapeComponent>())
                {
                    ref readonly var bShp = ref b.Get<ShapeComponent>();

                    if (!(c2.Radius + bShp.Radius >= (bPhy.Position - c1.Position).Length()))
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
                    var col = new CollisionComponent(ntt, b);
                    ntt.Add(ref col);
                    b.Add(ref col);
                }
            }
        }
    }
}