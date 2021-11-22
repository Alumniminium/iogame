using iogame.ECS;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    // public class NetworkingSystem : PixelSystem<NetworkingComponent>
    // {
    //     public NetworkingSystem() : base(Environment.ProcessorCount)
    //     {
    //         Name = "Networking System";
    //         PerformanceMetrics.RegisterSystem(Name);
    //     }
    // }
    public class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public float timeAcc = 0f;
        public ViewportSystem() : base(Environment.ProcessorCount)
        {
            Name = "Viewport System";
            PerformanceMetrics.RegisterSystem(Name);
        }
        public override void Update(float dt, List<PixelEntity> Entities)
        {
            timeAcc += dt;
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                if (vwp.EntitiesVisibleLastSync == null)
                    vwp.EntitiesVisibleLastSync = new();
                if (vwp.EntitiesVisible == null)
                    vwp.EntitiesVisible = new();

                var viewport = new System.Drawing.Rectangle((int)pos.Position.X - vwp.ViewDistance / 2, (int)pos.Position.Y - vwp.ViewDistance / 2, vwp.ViewDistance, vwp.ViewDistance);
                vwp.EntitiesVisible.Clear();
                CollisionDetection.Tree.GetObjects(viewport, vwp.EntitiesVisible);

                var shp = PixelWorld.GetAttachedShapeEntity(ref entity);
                if (shp is Player player)
                {
                    foreach (var visibleLast in vwp.EntitiesVisibleLastSync)
                    {
                        if (vwp.EntitiesVisible.Contains(visibleLast))
                        {
                            if (visibleLast.PositionComponent.LastPosition != visibleLast.PositionComponent.Position)
                                player.Send(MovementPacket.Create(visibleLast.EntityId, in visibleLast.PositionComponent.Position, in visibleLast.VelocityComponent.Velocity));
                        }
                        else
                            player.Send(StatusPacket.Create(visibleLast.EntityId, 0, StatusType.Alive));
                    }
                    foreach (var visible in vwp.EntitiesVisible)
                    {
                        if (!vwp.EntitiesVisibleLastSync.Contains(visible))
                        {
                            if (visible is not Player && visible is not Boid && visible is not Bullet)
                                player.Send(ResourceSpawnPacket.Create(visible));
                            else
                                player.Send(SpawnPacket.Create(visible));

                        }
                    }
                    vwp.EntitiesVisibleLastSync.Clear();
                    vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);

                    if (player.PositionComponent.LastPosition != player.PositionComponent.Position)
                        player.Send(MovementPacket.Create(player.EntityId, in player.PositionComponent.Position, in player.VelocityComponent.Velocity));
                }
            }
        }
    }
}