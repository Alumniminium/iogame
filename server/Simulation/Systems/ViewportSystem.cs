using System.Drawing;
using iogame.ECS;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;

namespace iogame.Simulation.Systems
{
    public class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", Environment.ProcessorCount) { }
        public override void Update(float dt, List<PixelEntity> Entities)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                var entity = Entities[i];
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                var viewport = new Rectangle((int)pos.Position.X - vwp.ViewDistance / 2, (int)pos.Position.Y - vwp.ViewDistance / 2, vwp.ViewDistance, vwp.ViewDistance);
                var visible = Game.Tree.GetObjects(viewport);
                vwp.EntitiesVisible = visible.ToArray();

                if (entity.IsPlayer())
                {
                    ref readonly var net = ref entity.Get<NetworkComponent>();
                    ref readonly var vel = ref entity.Get<VelocityComponent>();

                    foreach (var visibleLast in vwp.EntitiesVisibleLastSync)
                    {
                        ref readonly var other = ref PixelWorld.GetEntity(visibleLast.EntityId);

                        if (other.Has<PositionComponent>())
                        {
                            ref readonly var otherPos = ref other.Get<PositionComponent>();
                            if (vwp.EntitiesVisible.Contains(visibleLast))
                            {
                                if (otherPos.LastPosition != otherPos.Position)
                                {
                                    var otherVel = other.Has<VelocityComponent>() ? other.Get<VelocityComponent>() : default;
                                    entity.NetSync(MovementPacket.Create(other.EntityId, in otherPos.Position, in otherVel.Velocity));
                                }
                            }
                            else
                                entity.NetSync(StatusPacket.CreateDespawn(other.EntityId));
                        }
                        else
                            entity.NetSync(StatusPacket.CreateDespawn(other.EntityId));
                    }

                    for (int k = 0; k < vwp.EntitiesVisible.Length; k++)
                    {
                        if (!PixelWorld.EntityExists(vwp.EntitiesVisible[k].EntityId))
                            continue;
                        ref var other = ref PixelWorld.GetEntity(vwp.EntitiesVisible[k].EntityId);
                        bool visibleLastFrame = false;

                        for (int j = 0; j < vwp.EntitiesVisibleLastSync.Length; j++)
                            if (other.EntityId == vwp.EntitiesVisibleLastSync[j].EntityId)
                                visibleLastFrame = true;

                        if (visibleLastFrame)
                        {
                            if (pos.LastPosition != pos.Position)
                                entity.NetSync(MovementPacket.Create(entity.EntityId, in pos.Position, in vel.Velocity));
                        }
                        else
                        {
                            if (other.IsFood())
                            {
                                if (other.Has<ShapeComponent, PositionComponent, VelocityComponent>())
                                    entity.NetSync(ResourceSpawnPacket.Create(ref other));
                            }
                            else if (other.Has<PositionComponent, ShapeComponent, PhysicsComponent, VelocityComponent, SpeedComponent>())
                                entity.NetSync(SpawnPacket.Create(ref other));
                        }
                    }
                    vwp.EntitiesVisibleLastSync = vwp.EntitiesVisible;
                }
            }
        }
    }
}