using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct EnergyComponent(int entityId, float chargeRate, float availableCharge, float batteryCapacity)
{
    public readonly int EntityId = entityId;
    public float DiscargeRateAcc = 0;
    public float DiscargeRate;
    public float ChargeRate = chargeRate;
    public float AvailableCharge = availableCharge;
    public float BatteryCapacity = batteryCapacity;
    public uint ChangedTick = Game.CurrentTick;

    public override int GetHashCode() => EntityId;
}