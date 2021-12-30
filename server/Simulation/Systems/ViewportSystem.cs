using server.ECS;
using server.Simulation.Components;
using System.Diagnostics;

namespace server.Simulation.Systems
{
    public class PassiveViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public PassiveViewportSystem() : base("Passive Viewport Sys", threads: Environment.ProcessorCount) { }

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                var shpEntity = PixelWorld.GetAttachedShapeEntity(in entity);
                ref var pos = ref entity.Get<PhysicsComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                vwp.AddedEntities.Clear();
                vwp.RemovedEntities.Clear();
                vwp.ChangedEntities.Clear();
                vwp.EntitiesVisibleLastSync.Clear();
                vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);
                vwp.EntitiesVisible.Clear();

                if (pos.Position == pos.LastPosition)
                    continue;

                var rect = shpEntity.Rect;
                rect.X = pos.Position.X - shpEntity.Rect.Width / 2;
                rect.X = pos.Position.Y - shpEntity.Rect.Height / 2;
                shpEntity.Rect = rect;

                vwp.Viewport.X = pos.Position.X - vwp.ViewDistance / 2;
                vwp.Viewport.Y = pos.Position.Y - vwp.ViewDistance / 2;

                lock (Game.Tree)
                    if(!Game.Tree.Move(shpEntity))
                        Debugger.Break();
            }
        }
    }

    public class ViewportSystem : PixelSystem<PhysicsComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", Environment.ProcessorCount) { }

        protected override bool MatchesFilter(in PixelEntity entity) => (entity.IsPlayer() || entity.IsNpc()) && base.MatchesFilter(entity);

        protected override void Update(float dt, Span<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity = ref entities[i];
                var shpEntity = PixelWorld.GetAttachedShapeEntity(in entity);
                ref var pos = ref entity.Get<PhysicsComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                // if (pos.Position == pos.LastPosition)
                //     continue;
                
                lock (Game.Tree)
                    Game.Tree.GetObjects(vwp.Viewport, vwp.EntitiesVisible);

                for (int x = 0; x < vwp.EntitiesVisible.Count; x++)
                {
                    var visible = vwp.EntitiesVisible[x];
                    ref readonly var vPos = ref visible.Entity.Get<PhysicsComponent>();
                    ref readonly var vVwp = ref visible.Entity.Get<ViewportComponent>();

                    if (!vwp.EntitiesVisibleLastSync.Contains(visible))
                        vwp.AddedEntities.Add(visible);
                    else
                        vwp.ChangedEntities.Add(visible);

                    vVwp.ChangedEntities.Add(shpEntity);
                }

                for (int x = 0; x < vwp.EntitiesVisibleLastSync.Count; x++)
                {
                    var visibleLast = vwp.EntitiesVisibleLastSync[x];

                    if (!vwp.EntitiesVisible.Contains(visibleLast))
                        {
                            vwp.RemovedEntities.Add(visibleLast);
                            
                            if(!PixelWorld.EntityExists(visibleLast.Entity))
                                continue;

                            ref readonly var vVwp = ref visibleLast.Entity.Get<ViewportComponent>();
                            vVwp.EntitiesVisible.Remove(shpEntity);
                            vVwp.AddedEntities.Remove(shpEntity);
                            vVwp.ChangedEntities.Remove(shpEntity);
                        }
                }
            }
        }
    }
}