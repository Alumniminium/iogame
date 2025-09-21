using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class ShieldSystem : NttSystem<ShieldComponent, EnergyComponent>
{
    public ShieldSystem() : base("Shield System", threads: 1) { }

    public override void Update(in NTT ntt, ref ShieldComponent shi, ref EnergyComponent nrg)
    {
        if (ntt.Has<RespawnTagComponent>())
            return;

        if (!shi.PowerOn)
        {
            shi.Charge = 0;
            return;
        }

        shi.LastDamageTime += TimeSpan.FromSeconds(DeltaTime);
        var lastCharge = shi.Charge;
        var powerDraw = shi.PowerUse;

        if (shi.Charge < 0)
            shi.Charge = 0;

        var chargePercent = shi.Charge / shi.MaxCharge;
        shi.Radius = Math.Max(shi.MinRadius, shi.TargetRadius * chargePercent);

        if (shi.LastDamageTime > shi.RechargeDelay)
        {
            if (!ntt.Has<CollisionComponent>())
            {
                if (shi.Charge < shi.MaxCharge && nrg.AvailableCharge >= powerDraw)
                    shi.Charge += Math.Clamp(shi.RechargeRate * DeltaTime, 0, shi.MaxCharge - shi.Charge);
            }
        }

        nrg.DiscargeRateAcc += powerDraw;
        if (shi.Charge != lastCharge)
            shi.ChangedTick = NttWorld.Tick;
    }
}