using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.IsPlayer() && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref NetSyncComponent sync)
        {
            SelfUpdate(in ntt);

            if(ntt.IsPlayer())
            {
                ref readonly var vwp = ref ntt.Get<ViewportComponent>();
                
                for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
                {
                    ref readonly var changedEntity = ref vwp.EntitiesVisible[x].Entity;
                    Update(in ntt, in changedEntity);
                }
            }
        }

        public void SelfUpdate(in PixelEntity ntt) => Update(in ntt, in ntt);
        public void Update(in PixelEntity ntt, in PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();

            if (syn.Fields.HasFlags(SyncThings.Position))
            {
                ref var phy = ref other.Get<PhysicsComponent>();
                if(Game.CurrentTick == phy.ChangedTick)
                    ntt.NetSync(MovementPacket.Create(in other, ref phy));
            }
            if (syn.Fields.HasFlags(SyncThings.Power))
            {
                ref readonly var eng = ref other.Get<EngineComponent>();
                if(Game.CurrentTick == eng.ChangedTick)
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)(eng.Throttle * 100f), StatusType.Throttle));
            }
            if (syn.Fields.HasFlags(SyncThings.Invenory))
            {
                ref readonly var inv = ref other.Get<InventoryComponent>();
                if(Game.CurrentTick == inv.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.TotalCapacity, StatusType.InventoryCapacity));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Triangles, StatusType.InventoryTriangles));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Squares, StatusType.InventorySquares));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Pentagons, StatusType.InventoryPentagons));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Health))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
                if(Game.CurrentTick == hlt.ChangedTick)
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