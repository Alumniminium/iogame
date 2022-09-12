using System;
using System.Linq;
using server.ECS;
using server.Simulation.Components;

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

            var collsisions = Game.Grid.GetEntitiesSameAndDirection(a).ToList();

            for (var k = 0; k < collsisions.Count; k++)
            {
                var b = collsisions[k];

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
        }
    }
}