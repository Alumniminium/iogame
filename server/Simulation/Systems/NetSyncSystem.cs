using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

public sealed class NetSyncSystem : NttSystem<NetSyncComponent>
{
    public NetSyncSystem() : base("NetSync System", threads: 1) { }

    protected override bool MatchesFilter(in NTT ntt) => ntt.Has<NetworkComponent>() && base.MatchesFilter(ntt);

    public override void Update(in NTT ntt, ref NetSyncComponent c1)
    {
        SelfUpdate(in ntt);

        if (!ntt.Has<NetworkComponent>())
            return;

        ref readonly var vwp = ref ntt.Get<ViewportComponent>();

        for (var x = 0; x < vwp.EntitiesVisible.Count; x++)
        {
            var changedEntity = vwp.EntitiesVisible[x];
            Update(in ntt, in changedEntity);
        }
    }

    public void SelfUpdate(in NTT ntt) => Update(in ntt, in ntt);

    public void Update(in NTT ntt, in NTT other)
    {
        ref readonly var syn = ref other.Get<NetSyncComponent>();
        if (syn.Fields.HasFlags(SyncThings.Battery))
        {
            ref readonly var energy = ref other.Get<EnergyComponent>();
            if (NttWorld.Tick == energy.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, energy.BatteryCapacity, StatusType.BatteryCapacity));
                ntt.NetSync(StatusPacket.Create(other, energy.AvailableCharge, StatusType.BatteryCharge));
                ntt.NetSync(StatusPacket.Create(other, energy.ChargeRate, StatusType.BatteryChargeRate));
                ntt.NetSync(StatusPacket.Create(other, energy.DiscargeRate, StatusType.BatteryDischargeRate));
            }
        }
        if (syn.Fields.HasFlag(SyncThings.Shield))
        {
            ref readonly var shi = ref other.Get<ShieldComponent>();
            if (NttWorld.Tick == shi.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, shi.Charge, StatusType.ShieldCharge));
                ntt.NetSync(StatusPacket.Create(other, shi.Radius, StatusType.ShieldRadius));

                if (ntt.Id == other)
                {
                    ntt.NetSync(StatusPacket.Create(other, shi.MaxCharge, StatusType.ShieldMaxCharge));
                    ntt.NetSync(StatusPacket.Create(other, shi.RechargeRate, StatusType.ShieldRechargeRate));
                    ntt.NetSync(StatusPacket.Create(other, shi.PowerUse, StatusType.ShieldPowerUse));
                    ntt.NetSync(StatusPacket.Create(other, shi.PowerUseRecharge, StatusType.ShieldPowerUseRecharge));
                }
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Position))
        {
            ref var phy = ref other.Get<PhysicsComponent>();

            if (NttWorld.Tick == phy.ChangedTick)
                ntt.NetSync(MovementPacket.Create(other, NttWorld.Tick, phy.Position, phy.LinearVelocity, phy.RotationRadians));
        }
        if (syn.Fields.HasFlags(SyncThings.Throttle))
        {
            ref readonly var eng = ref other.Get<EngineComponent>();
            if (NttWorld.Tick == eng.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, eng.Throttle * 100f, StatusType.Throttle));
                ntt.NetSync(StatusPacket.Create(other, eng.PowerUse * eng.Throttle, StatusType.EnginePowerDraw));
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Invenory))
        {
            ref readonly var inv = ref other.Get<InventoryComponent>();
            if (NttWorld.Tick == inv.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, inv.TotalCapacity, StatusType.InventoryCapacity));
                ntt.NetSync(StatusPacket.Create(other, inv.Triangles, StatusType.InventoryTriangles));
                ntt.NetSync(StatusPacket.Create(other, inv.Squares, StatusType.InventorySquares));
                ntt.NetSync(StatusPacket.Create(other, inv.Pentagons, StatusType.InventoryPentagons));
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Health))
        {
            ref readonly var hlt = ref other.Get<HealthComponent>();
            if (NttWorld.Tick == hlt.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, hlt.Health, StatusType.Health));
                ntt.NetSync(StatusPacket.Create(other, hlt.MaxHealth, StatusType.MaxHealth));
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Size))
        {
            ref var phy = ref other.Get<PhysicsComponent>();

            if (phy.SizeLastFrame != phy.Size)
            {
                phy.SizeLastFrame = phy.Size;
                ntt.NetSync(StatusPacket.Create(other, phy.Size, StatusType.Size));
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Level))
        {
            ref readonly var lvl = ref other.Get<LevelComponent>();
            if (NttWorld.Tick == lvl.ChangedTick)
            {
                ntt.NetSync(StatusPacket.Create(other, lvl.Level, StatusType.Level));
                ntt.NetSync(StatusPacket.Create(other, lvl.ExperienceToNextLevel, StatusType.ExperienceToNextLevel));
            }
        }
        if (syn.Fields.HasFlags(SyncThings.Experience))
        {
            ref readonly var lvl = ref other.Get<LevelComponent>();
            if (NttWorld.Tick == lvl.ChangedTick)
                ntt.NetSync(StatusPacket.Create(other, lvl.Experience, StatusType.Experience));
        }
    }
}