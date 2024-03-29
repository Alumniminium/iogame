using Packets;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

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

            for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
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
                ref readonly var energy = ref other.Get<EnergyComponent>();
                if (Game.CurrentTick == energy.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, energy.BatteryCapacity, StatusType.BatteryCapacity));
                    ntt.NetSync(StatusPacket.Create(other.Id, energy.AvailableCharge, StatusType.BatteryCharge));
                    ntt.NetSync(StatusPacket.Create(other.Id, energy.ChargeRate, StatusType.BatteryChargeRate));
                    ntt.NetSync(StatusPacket.Create(other.Id, energy.DiscargeRate, StatusType.BatteryDischargeRate));
                }
            }
            if (syn.Fields.HasFlag(SyncThings.Shield))
            {
                ref readonly var shi = ref other.Get<ShieldComponent>();
                if (Game.CurrentTick == shi.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, shi.Charge, StatusType.ShieldCharge));
                    ntt.NetSync(StatusPacket.Create(other.Id, shi.Radius, StatusType.ShieldRadius));

                    if(ntt.Id == other.Id)
                    {
                        ntt.NetSync(StatusPacket.Create(other.Id, shi.MaxCharge, StatusType.ShieldMaxCharge));
                        ntt.NetSync(StatusPacket.Create(other.Id, shi.RechargeRate, StatusType.ShieldRechargeRate));
                        ntt.NetSync(StatusPacket.Create(other.Id, shi.PowerUse, StatusType.ShieldPowerUse));
                        ntt.NetSync(StatusPacket.Create(other.Id, shi.PowerUseRecharge, StatusType.ShieldPowerUseRecharge));
                    }
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Position))
            {
                ref var phy = ref other.Get<PhysicsComponent>(); 
                
                if (Game.CurrentTick == phy.ChangedTick)
                    ntt.NetSync(MovementPacket.Create(other.Id, Game.CurrentTick, phy.Position, phy.RotationRadians));
            }
            if (syn.Fields.HasFlags(SyncThings.Throttle))
            {
                ref readonly var eng = ref other.Get<EngineComponent>();
                if (Game.CurrentTick == eng.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, eng.Throttle * 100f, StatusType.Throttle));
                    ntt.NetSync(StatusPacket.Create(other.Id, eng.PowerUse * eng.Throttle, StatusType.EnginePowerDraw));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Invenory))
            {
                ref readonly var inv = ref other.Get<InventoryComponent>();
                if (Game.CurrentTick == inv.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, inv.TotalCapacity, StatusType.InventoryCapacity));
                    ntt.NetSync(StatusPacket.Create(other.Id, inv.Triangles, StatusType.InventoryTriangles));
                    ntt.NetSync(StatusPacket.Create(other.Id, inv.Squares, StatusType.InventorySquares));
                    ntt.NetSync(StatusPacket.Create(other.Id, inv.Pentagons, StatusType.InventoryPentagons));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Health))
            {
                ref readonly var hlt = ref other.Get<HealthComponent>();
                if (Game.CurrentTick == hlt.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, hlt.Health, StatusType.Health));
                    ntt.NetSync(StatusPacket.Create(other.Id, hlt.MaxHealth, StatusType.MaxHealth));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Size))
            {
                ref var phy = ref other.Get<PhysicsComponent>();

                if (phy.SizeLastFrame != phy.Size)
                {
                    phy.SizeLastFrame = phy.Size;
                    ntt.NetSync(StatusPacket.Create(other.Id, phy.Size, StatusType.Size));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Level))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if (Game.CurrentTick == lvl.ChangedTick)
                {
                    ntt.NetSync(StatusPacket.Create(other.Id, lvl.Level, StatusType.Level));
                    ntt.NetSync(StatusPacket.Create(other.Id, lvl.ExperienceToNextLevel, StatusType.ExperienceToNextLevel));
                }
            }
            if (syn.Fields.HasFlags(SyncThings.Experience))
            {
                ref readonly var lvl = ref other.Get<LevelComponent>();
                if (Game.CurrentTick == lvl.ChangedTick)
                    ntt.NetSync(StatusPacket.Create(other.Id, lvl.Experience, StatusType.Experience));
            }
        }
    }
}