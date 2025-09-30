using System;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Shield, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShieldComponent(float charge, float maxCharge, float powerUseIdle, float radius, float minRadius, float rechargeRate, TimeSpan rechargeDelay)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public bool PowerOn = true;
    public bool LastPowerOn = true;
    public float Charge = charge;
    public float MaxCharge = maxCharge;
    public float PowerUse = powerUseIdle;
    public float PowerUseRecharge = powerUseIdle * 2.5f;
    public float Radius = radius;
    public float MinRadius = minRadius;
    public float TargetRadius = radius;
    public float RechargeRate = rechargeRate;
    public long RechargeDelayTicks = (long)(rechargeDelay.TotalSeconds * NttWorld.TargetTps);
    public long LastDamageTimeTicks = 0;
}