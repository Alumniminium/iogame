using System;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct ShieldComponent(NTT EntityId, float charge, int maxCharge, float powerUseIdle, float radius, float minRadius, float rechargeRate, TimeSpan rechargeDelay)
{
    public readonly NTT EntityId = EntityId;
    public bool PowerOn = true;
    public bool LastPowerOn = true;
    public float Charge = charge;
    public readonly int MaxCharge = maxCharge;
    public readonly float PowerUse = powerUseIdle;
    public readonly float PowerUseRecharge = powerUseIdle * 2.5f;
    internal float Radius = radius;
    internal readonly float MinRadius = minRadius;
    internal readonly float TargetRadius = radius;
    public long ChangedTick = NttWorld.Tick;
    internal float RechargeRate = rechargeRate;
    public TimeSpan RechargeDelay = rechargeDelay;
    public TimeSpan LastDamageTime;


}