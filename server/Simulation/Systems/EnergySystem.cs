using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class EnergySystem : NttSystem<EnergyComponent>
{
    public EnergySystem() : base("Energy System", threads: 1) { }

    public override void Update(in NTT a, ref EnergyComponent nrg)
    {
        var lastCharge = nrg.AvailableCharge;
        nrg.DiscargeRate = nrg.DiscargeRateAcc;

        if (nrg.DiscargeRateAcc > 0)
        {
            nrg.AvailableCharge -= Math.Clamp(nrg.DiscargeRate * DeltaTime, 0, nrg.AvailableCharge);
        }

        if (nrg.AvailableCharge < nrg.BatteryCapacity)
        {
            nrg.AvailableCharge += Math.Clamp(nrg.ChargeRate * DeltaTime, 0, nrg.BatteryCapacity - nrg.AvailableCharge);
        }

        nrg.DiscargeRateAcc = 0;

        if (lastCharge != nrg.AvailableCharge)
            nrg.ChangedTick = NttWorld.Tick;

    }
}