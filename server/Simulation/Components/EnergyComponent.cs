using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct EnergyComponent
    {
        public readonly int EntityId;
        public float DiscargeRateAcc;
        public float DiscargeRate;
        public float ChargeRate;
        public float AvailableCharge;
        public float BatteryCapacity;
        public uint ChangedTick;

        public EnergyComponent(int entityId, float chargeRate, float availableCharge, float batteryCapacity)
        {
            EntityId = entityId;
            DiscargeRateAcc = 0;
            ChargeRate = chargeRate;
            AvailableCharge = availableCharge;
            BatteryCapacity = batteryCapacity;
            ChangedTick = Game.CurrentTick;
        }
        public override int GetHashCode() => EntityId;
    }
}