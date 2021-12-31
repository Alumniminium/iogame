using server.ECS;
using server.Helpers;
using server.Simulation.Components;
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
                ref var pos = ref other.Get<PhysicsComponent>();

                if (pos.LastSyncedPosition == pos.Position)
                    return;

                ntt.NetSync(MovementPacket.Create(in other));
            }
            if (syn.Fields.HasFlags(SyncThings.Health))
            {
                ref var hlt = ref other.Get<HealthComponent>();

                if (Math.Abs(hlt.LastHealth - hlt.Health) < 0f)
                    return;

                hlt.LastHealth = hlt.Health;
                ntt.NetSync(StatusPacket.Create(other.Id, (uint)hlt.Health, StatusType.Health));
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