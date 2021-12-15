using System.Drawing;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", Environment.ProcessorCount) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref readonly var pos = ref entity.Get<PositionComponent>();
                ref var vwp = ref entity.Get<ViewportComponent>();

                if(entity.IsFood() && pos.Position == pos.LastPosition)
                    continue;

                vwp.Viewport.X = pos.Position.X - vwp.ViewDistance /2;
                vwp.Viewport.Y = pos.Position.Y - vwp.ViewDistance /2;
                vwp.EntitiesVisible.Clear();

                Game.Tree.GetObjects(vwp.Viewport, vwp.EntitiesVisible);

                if (!entity.IsPlayer()) 
                    continue;

                ref readonly var vel = ref entity.Get<VelocityComponent>();

                if(pos.Position != pos.LastPosition)
                    entity.NetSync(MovementPacket.Create(entity.EntityId,in pos.Position, in vel.Velocity));
                
                for (var l = 0; l < vwp.EntitiesVisibleLastSync.Count; l++)
                {
                    var id = vwp.EntitiesVisibleLastSync[l].Entity.EntityId;
                    if(!PixelWorld.EntityExists(id))
                        continue;

                    ref readonly var other = ref PixelWorld.GetEntity(id);
                    
                    var visibleNow = false;
                    for (var j = 0; j < vwp.EntitiesVisible.Count; j++)
                        if (other.EntityId == vwp.EntitiesVisible[j].Entity.EntityId)
                            {
                                visibleNow = true;
                                break;
                            }

                    if (visibleNow)
                    {
                        ref readonly var otherPos = ref other.Get<PositionComponent>();
                        if (otherPos.LastPosition == otherPos.Position) 
                            continue;
                        var otherVel = other.Has<VelocityComponent>() ? other.Get<VelocityComponent>() : default;
                        entity.NetSync(MovementPacket.Create(other.EntityId, in otherPos.Position, in otherVel.Velocity));
                    }
                    else
                        entity.NetSync(StatusPacket.CreateDespawn(other.EntityId));
                }

                for (var l = 0; l < vwp.EntitiesVisible.Count; l++)
                {
                    var other = vwp.EntitiesVisible[l].Entity;
                 
                    if(!PixelWorld.EntityExists(in other))
                        continue;

                    if(vwp.EntitiesVisibleLastSync.Contains(PixelWorld.GetAttachedShapeEntity(in other)))
                        continue;

                    if(other.IsFood())
                        entity.NetSync(ResourceSpawnPacket.Create(in other));
                    else
                        entity.NetSync(SpawnPacket.Create(in other));

                }
                vwp.EntitiesVisibleLastSync.Clear();
                vwp.EntitiesVisibleLastSync.AddRange(vwp.EntitiesVisible);
            }
        }
    }
}