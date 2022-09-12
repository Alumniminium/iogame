using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class CollisionDetector : PixelSystem<PhysicsComponent, ShapeComponent,ViewportComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: Environment.ProcessorCount) { }
        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ShapeComponent shp, ref ViewportComponent vwp)
        {
            if (phy.Position == phy.LastPosition || ntt.Has<CollisionComponent>())
                return;
            
            for (var k = 0; k < vwp.EntitiesVisible.Count; k++)
            {
                var b = vwp.EntitiesVisible[k];

                if (b.Id == ntt.Id)
                    continue;

                if(b.Type == EntityType.Drop && ntt.Type != EntityType.Player)
                    continue;

                ref var bPhy = ref b.Get<PhysicsComponent>();
                bool collided = false;

                if (b.Has<ShapeComponent>())
                {
                    ref readonly var bShp = ref b.Get<ShapeComponent>();

                    if (!(shp.Radius + bShp.Radius >= (bPhy.Position - phy.Position).Length()))
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