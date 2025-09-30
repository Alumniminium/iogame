using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Energy, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnergyComponent(float chargeRate, float availableCharge, float batteryCapacity)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public float DiscargeRateAcc = 0;
    public float DiscargeRate = 0;
    public float ChargeRate = chargeRate;
    public float AvailableCharge = availableCharge;
    public float BatteryCapacity = batteryCapacity;
}