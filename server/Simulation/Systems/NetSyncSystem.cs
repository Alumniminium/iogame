using System.Diagnostics.Contracts;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity entity) => entity.IsPlayer() && base.MatchesFilter(entity);

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                ref var vwp = ref entity.Get<ViewportComponent>();

                SelfUpdate(ref entity);

                for(int x =0; x < vwp.AddedEntities.Count; x++)
                {
                    var addedEntity = vwp.AddedEntities[x];
                    entity.NetSync(SpawnPacket.Create(in addedEntity.Entity));
                }
                for(int x =0; x < vwp.RemovedEntities.Count; x++)
                {
                    var removedEntity = vwp.RemovedEntities[x];
                    entity.NetSync(StatusPacket.CreateDespawn(removedEntity.Entity.Id));
                }   
                for(int x =0; x < vwp.ChangedEntities.Count; x++)
                {
                    var changedEntity = vwp.ChangedEntities[x];
                    Update(ref entity, ref changedEntity.Entity);
                }
            }
        }

        public void SelfUpdate(ref PixelEntity entity) => Update(ref entity, ref entity);
        public void Update(ref PixelEntity entity, ref PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();

            if (syn.Fields.HasFlag(SyncThings.Position))
            {
                ref var pos = ref other.Get<PositionComponent>();

                if (pos.LastSyncedPosition == pos.Position)
                    return;
                // if (Math.Abs((pos.Position - pos.LastPosition).Length()) < 0.1f)
                //     return;

                pos.LastSyncedPosition = pos.Position;
                entity.NetSync(MovementPacket.Create(other.Id, ref pos.Position));
            }
            if (syn.Fields.HasFlag(SyncThings.Health))
            {
                ref var hlt = ref other.Get<HealthComponent>();

                if (hlt.LastHealth == hlt.Health)
                    return;

                hlt.LastHealth = hlt.Health;
                entity.NetSync(StatusPacket.Create(other.Id, (uint)hlt.Health, StatusType.Health));
            }
            if (syn.Fields.HasFlag(SyncThings.Size))
            {
                ref var shp = ref other.Get<ShapeComponent>();

                if (shp.SizeLastFrame == shp.Size)
                    return;

                shp.SizeLastFrame = shp.Size;
                entity.NetSync(StatusPacket.Create(other.Id, shp.Size, StatusType.Size));
            }
        }
    }
}