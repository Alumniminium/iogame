using System;
using System.Collections.Generic;
using System.Drawing;
using server.ECS;
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

                var viewport = new RectangleF(pos.Position.X - vwp.ViewDistance/2, pos.Position.Y - vwp.ViewDistance/2, vwp.ViewDistance, vwp.ViewDistance);

                var visible = Game.Tree.GetObjects(viewport);
                vwp.EntitiesVisible = visible.ToArray();

                if (!entity.IsPlayer()) 
                    continue;

                ref readonly var vel = ref entity.Get<VelocityComponent>();

                if(pos.Position != pos.LastPosition)
                    entity.NetSync(MovementPacket.Create(entity.EntityId,in pos.Position, in vel.Velocity));
                
                for (var l = 0; l < vwp.EntitiesVisibleLastSync.Length; l++)
                {
                    var id = vwp.EntitiesVisibleLastSync[l].Entity.EntityId;
                    if(!PixelWorld.EntityExists(id))
                        continue;

                    ref readonly var other = ref PixelWorld.GetEntity(id);
                    
                    var visibleNow = false;
                    for (var j = 0; j < vwp.EntitiesVisible.Length; j++)
                        if (other.EntityId == vwp.EntitiesVisible[j].Entity.EntityId)
                            visibleNow = true;

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

                for (var l = 0; l < vwp.EntitiesVisible.Length; l++)
                {
                    var id = vwp.EntitiesVisible[l].Entity.EntityId;
                 
                    if(Contains(vwp.EntitiesVisibleLastSync,id))
                        continue;
                    if(!PixelWorld.EntityExists(id))
                        continue;
                        
                    ref var other = ref PixelWorld.GetEntity(id);
                    if(other.IsFood())
                        entity.NetSync(ResourceSpawnPacket.Create(ref other));
                    else
                        entity.NetSync(SpawnPacket.Create(ref other));

                }

                vwp.EntitiesVisibleLastSync = vwp.EntitiesVisible;
            }
        }

        private static bool Contains(ShapeEntity[] array, int id)
        {
            for(var i = 0; i < array.Length; i++)
                if(array[i].Entity.EntityId == id)
                    return true;
            return false;
        }
    }
}