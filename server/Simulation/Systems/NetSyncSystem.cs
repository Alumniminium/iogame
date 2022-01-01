using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class NetSyncSystem : PixelSystem<NetSyncComponent, ViewportComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.IsPlayer() && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref NetSyncComponent sync, ref ViewportComponent vwp)
        {
            SelfUpdate(in ntt);

            for (int x = 0; x < vwp.AddedEntities.Count; x++)
            {
                var addedEntity = vwp.AddedEntities[x];
                ntt.NetSync(SpawnPacket.Create(in addedEntity));
            }
            for (int x = 0; x < vwp.RemovedEntities.Count; x++)
            {
                var removedEntity = vwp.RemovedEntities[x];
                if(removedEntity.Id == ntt.Id)
                    continue;
                ntt.NetSync(StatusPacket.CreateDespawn(removedEntity.Id));
            }
            for (int x = 0; x < vwp.EntitiesVisible.Count; x++)
            {
                ref readonly var changedEntity = ref vwp.EntitiesVisible[x].Entity;
                Update(in ntt, in changedEntity);
            }

            vwp.AddedEntities.Clear();
            vwp.RemovedEntities.Clear();
        }

        public void SelfUpdate(in PixelEntity ntt) => Update(in ntt, in ntt);
        public void Update(in PixelEntity ntt, in PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();

            if (syn.Fields.HasFlags(SyncThings.Position))
            {
                ref readonly var phy = ref other.Get<PhysicsReplicationComponent>();
                if(Game.CurrentTick == phy.CreatedTick)
                    ntt.NetSync(MovementPacket.Create(in other, in phy));
            }
            if (syn.Fields.HasFlags(SyncThings.Health))
            {
                ref readonly var hlt = ref other.Get<HealthReplicationComponent>();
                if(Game.CurrentTick == hlt.CreatedTick)
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)hlt.ClientHealth, StatusType.Health));
            }
            if (syn.Fields.HasFlags(SyncThings.Size))
            {
                ref var shp = ref other.Get<ShapeComponent>();

                if (shp.SizeLastFrame == shp.Size)
                    return;

                shp.SizeLastFrame = shp.Size;
                ntt.NetSync(StatusPacket.Create(other.Id, shp.Size, StatusType.Size));
            }
        }
    }
}