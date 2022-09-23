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
        internal float Radius;
        internal readonly float MinRadius;
        internal readonly float TargetRadius;
        public uint ChangedTick;
        internal float RechargeRate;
        public TimeSpan RechargeDelay;
        public TimeSpan LastDamageTime;

        public ShieldComponent(float charge, int maxCharge, float powerUseIdle, float radius, float minRadius, float rechargeRate, TimeSpan rechargeDelay)
        {
            Charge = charge;
            MaxCharge = maxCharge;
            PowerUse = powerUseIdle;
            PowerUseRecharge = powerUseIdle * 2.5f;
            Radius = radius;
            MinRadius = minRadius;
            TargetRadius = radius;
            RechargeRate = rechargeRate;
            ChangedTick = Game.CurrentTick;
            RechargeDelay = rechargeDelay;
        }
    }
}