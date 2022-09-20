using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public sealed class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type == EntityType.Player && base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref NetSyncComponent c1)
        {
            SelfUpdate(in ntt);

            if (ntt.Type != EntityType.Player)
                return;

            ref readonly var vwp = ref ntt.Get<ViewportComponent>();

            for (var x = 0; x < vwp.EntitiesVisible.Length; x++)
            {
                var changedEntity = vwp.EntitiesVisible[x];
                Update(in ntt, in changedEntity);
            }
        }

        public void SelfUpdate(in PixelEntity ntt) => Update(in ntt, in ntt);

        public void Update(in PixelEntity ntt, in PixelEntity other)
        {
            ref readonly var syn = ref other.Get<NetSyncComponent>();
            if (syn.Fields.HasFlags(SyncThings.Battery))
            {
                if (Game.CurrentTick % 3 == 0)
                {
                    ref readonly var energy = ref other.Get<EnergyComponent>();
                    if (Game.CurrentTick == energy.ChangedTick)
                    {
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)energy.BatteryCapacity, StatusType.BatteryCapacity));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)energy.AvailableCharge, StatusType.BatteryCharge));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)energy.ChargeRate, StatusType.BatteryChargeRate));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)energy.DiscargeRate, StatusType.BatteryDischargeRate));
                    }
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Shield))
            {
                ref readonly var shi = ref other.Get<ShieldComponent>();
                if (Game.CurrentTick == shi.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.Charge, StatusType.ShieldCharge));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.MaxCharge, StatusType.ShieldMaxCharge));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.RechargeRate, StatusType.ShieldRechargeRate));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.PowerUse, StatusType.ShieldPowerUse));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.PowerUseRecharge, StatusType.ShieldPowerUseRecharge));
                    ntt.NetSync(StatusPacket.Create(other.Id, (uint)shi.Radius, StatusType.ShieldRadius));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Position))
            {
                ref var phy = ref other.Get<PhysicsComponent>();
                if (Game.CurrentTick == phy.ChangedTick)
                    ntt.NetSync(MovementPacket.Create(in other, ref phy));
            }
            if (syn.Fields.HasFlags(SyncThings.Throttle))
            {
                if (Game.CurrentTick % 2 == 0)
                {
                    ref readonly var eng = ref other.Get<EngineComponent>();
                    if (Game.CurrentTick == eng.ChangedTick)
                    {
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)(eng.Throttle * 100f), StatusType.Throttle));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)(eng.PowerUse * eng.Throttle), StatusType.EnginePowerDraw));
                    }
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Invenory))
            {
                if (Game.CurrentTick % 30 == 0)
                {
                    ref readonly var inv = ref other.Get<InventoryComponent>();
                    if (Game.CurrentTick == inv.ChangedTick)
                    {
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.TotalCapacity, StatusType.InventoryCapacity));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Triangles, StatusType.InventoryTriangles));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Squares, StatusType.InventorySquares));
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)inv.Pentagons, StatusType.InventoryPentagons));
                    }
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Health))
            {
                if (Game.CurrentTick % 2 == 0)
                {
                    ref readonly var hlt = ref other.Get<HealthComponent>();
                    if (Game.CurrentTick == hlt.ChangedTick)
                        ntt.NetSync(StatusPacket.Create(other.Id, (uint)hlt.Health, StatusType.Health));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Size))
            {
                ref var phy = ref other.Get<PhysicsComponent>();

                if (phy.SizeLastFrame == phy.Size)
                    return;

                phy.SizeLastFrame = phy.Size;
                ntt.NetSync(StatusPacket.Create(other.Id, phy.Size, StatusType.Size));
            }
        }
    }
}