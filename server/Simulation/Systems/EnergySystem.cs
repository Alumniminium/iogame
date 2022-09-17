using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class EnergySystem : PixelSystem<EnergyComponent>
    {
        public EnergySystem() : base("Energy System", threads: 1) { }

        public override void Update(in PixelEntity a, ref EnergyComponent energy)
        {
            energy.DiscargeRate = energy.DiscargeRateAcc;
            if (energy.AvailableCharge < energy.BatteryCapacity)
            {
                energy.AvailableCharge += Math.Clamp(energy.ChargeRate * deltaTime, 0, energy.BatteryCapacity - energy.AvailableCharge);
                energy.ChangedTick = Game.CurrentTick;
            }
            if (energy.DiscargeRateAcc > 0)
            {
                energy.AvailableCharge -= Math.Clamp(energy.DiscargeRate * deltaTime, 0, energy.AvailableCharge);
                energy.ChangedTick = Game.CurrentTick;
            }
            energy.DiscargeRateAcc = 0;
        }
    }
}