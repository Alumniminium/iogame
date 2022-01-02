using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{

    public class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", Environment.ProcessorCount) { }

        protected override bool MatchesFilter(in PixelEntity ntt) => (ntt.IsPlayer() || ntt.IsNpc()) && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
        {
            vwp.EntitiesVisible.Clear();

            Game.Tree.GetObjects(vwp.Viewport, vwp.EntitiesVisible);

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);

            for (int x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                var visible = vwp.EntitiesVisible[x];

                if (!vwp.EntitiesVisibleLastSync.Contains(visible))
                    vwp.AddedEntities.Add(in visible.Entity);

                if(!visible.Entity.IsFood())
                    continue;

                ref readonly var vVwp = ref visible.Entity.Get<ViewportComponent>();
                if (!vVwp.EntitiesVisible.Contains(shpEntity))
                    vVwp.EntitiesVisible.Add(shpEntity);
            }

            for (int x = 0; x < vwp.EntitiesVisibleLastSync.Count; x++)
            {
                var visibleLast = vwp.EntitiesVisibleLastSync[x];

                if (!vwp.EntitiesVisible.Contains(visibleLast))
                {
                    vwp.RemovedEntities.Add(in visibleLast.Entity);

                    if(!visibleLast.Entity.IsFood())
                        continue;

                    ref readonly var vVwp = ref visibleLast.Entity.Get<ViewportComponent>();
                    vVwp.EntitiesVisible.Remove(shpEntity);
                }
            }
        }
    }
}