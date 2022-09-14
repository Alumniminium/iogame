using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class CollisionDetector : PixelSystem<PhysicsComponent, ShapeComponent, ViewportComponent>
    {
        public CollisionDetector() : base("Collision Detector", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Pickable && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ShapeComponent shp, ref ViewportComponent vwp)
        {
            if (phy.Position == phy.LastPosition || ntt.Has<CollisionComponent>())
                return;

            for (var k = 0; k < vwp.EntitiesVisible.Length; k++)
            {
                var b = vwp.EntitiesVisible[k];

                if (b.Id == ntt.Id || b.Has<CollisionComponent>())
                    continue;

                ref readonly var bPhy = ref b.Get<PhysicsComponent>();
                ref readonly var bShp = ref b.Get<ShapeComponent>();

                if (!(shp.Radius + bShp.Radius >= (bPhy.Position - phy.Position).Length()))
                    continue;

                var col = new CollisionComponent(ntt, b);
                ntt.Add(ref col);
                b.Add(ref col);
            }
        }
    }
}