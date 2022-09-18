using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EnergyComponent
    {
        public float DiscargeRateAcc;
        public float DiscargeRate;
        public float ChargeRate;
        public float AvailableCharge;
        public float BatteryCapacity;
        public uint ChangedTick;

        public EnergyComponent(float chargeRate, float availableCharge, float batteryCapacity)
        {
            DiscargeRateAcc = 0;
            ChargeRate = chargeRate;
            AvailableCharge = availableCharge;
            BatteryCapacity = batteryCapacity;
        }
    }
}