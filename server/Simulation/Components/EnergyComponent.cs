using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct EnergyComponent(NTT EntityId, float chargeRate, float availableCharge, float batteryCapacity)
{
    public readonly NTT EntityId = EntityId;
    public float DiscargeRateAcc = 0;
    public float DiscargeRate;
    public float ChargeRate = chargeRate;
    public float AvailableCharge = availableCharge;
    public float BatteryCapacity = batteryCapacity;
    public long ChangedTick = NttWorld.Tick;


}