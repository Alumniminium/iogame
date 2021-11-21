using iogame.ECS;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base(Environment.ProcessorCount)
        {
            Name = "ViewportSystem System";
            PerformanceMetrics.RegisterSystem(Name);
        }
        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                vwp.EntitiesVisible = CollisionDetection.Grid.GetObjects(new System.Drawing.Rectangle((int)pos.Position.X - vwp.ViewDistance/2,(int)pos.Position.Y - vwp.ViewDistance/2,vwp.ViewDistance,vwp.ViewDistance));

                var shp = PixelWorld.GetAttachedShapeEntity(ref entity);
                if (shp is Player player)
                {
                    foreach(var visible in vwp.EntitiesVisible)
                    {
                        if(vwp.EntitiesVisibleLastSync.Contains(visible))
                            player.Send(MovementPacket.Create(visible.EntityId, visible.PositionComponent.Position, visible.VelocityComponent.Velocity));
                        else
                            player.Send(SpawnPacket.Create(visible));
                    }
                    vwp.EntitiesVisibleLastSync.Clear();
                    vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);
                }
            }
        }
    }
}