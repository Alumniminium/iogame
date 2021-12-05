using System;
using System.Drawing;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", Environment.ProcessorCount/2) { }
        public override void Update(float dt, RefList<PixelEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                ref readonly var entity = ref entities[i];
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                var viewport = new Rectangle((int)pos.Position.X - vwp.ViewDistance / 2, (int)pos.Position.Y - vwp.ViewDistance / 2, vwp.ViewDistance, vwp.ViewDistance);
                var visible = Game.Tree.GetObjects(viewport);
                vwp.EntitiesVisible = visible.ToArray();

                if (entity.IsPlayer())
                {
                    ref readonly var vel = ref entity.Get<VelocityComponent>();

                    for (int l = 0; l < vwp.EntitiesVisibleLastSync.Length; l++)
                    {
                        ref readonly var other = ref PixelWorld.GetEntity(vwp.EntitiesVisibleLastSync[l].EntityId);

                        if (other.Has<PositionComponent>())
                        {
                            bool visibleLastFrame = false;

                            for (int j = 0; j < vwp.EntitiesVisible.Length; j++)
                                if (other.EntityId == vwp.EntitiesVisible[j].EntityId)
                                    visibleLastFrame = true;

                            if (visibleLastFrame)
                            {
                                ref readonly var otherPos = ref other.Get<PositionComponent>();
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