using System;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ShieldComponent
    {
        public float Charge;
        public readonly int MaxCharge;
        public readonly float PowerUse;
        public readonly float PowerUseRecharge;
        internal readonly float Radius;
        public uint ChangedTick;
        internal float RechargeRate;
        public uint LastDamageTick;
        public TimeSpan RechargeDelay;

        public ShieldComponent(float charge, int maxCharge, float powerUseIdle, float radius, float rechargeRate, TimeSpan rechargeDelay)
        {
            Charge = charge;
            MaxCharge = maxCharge;
            PowerUse = powerUseIdle;
            PowerUseRecharge = powerUseIdle * 2.5f;
            Radius = radius;
            RechargeRate = rechargeRate;
            ChangedTick = 0;
            RechargeDelay = rechargeDelay;
        }
    }
}