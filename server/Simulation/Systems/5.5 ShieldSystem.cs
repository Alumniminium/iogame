using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class ShieldSystem : PixelSystem<ShieldComponent, EnergyComponent>
    {
        public ShieldSystem() : base("Shield System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref ShieldComponent shi, ref EnergyComponent nrg)
        {
            if(ntt.Has<RespawnTagComponent>())
                return;
                
            shi.LastDamageTime += TimeSpan.FromSeconds(deltaTime);
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
                    if (shi.Charge < shi.MaxCharge)
                    {
                        shi.Charge += Math.Clamp(shi.RechargeRate * deltaTime, 0, shi.MaxCharge - shi.Charge);
                        powerDraw += shi.PowerUseRecharge;
                    }
                }
            }

            nrg.DiscargeRateAcc += powerDraw;
            if (shi.Charge != lastCharge)
                shi.ChangedTick = Game.CurrentTick;
        }
    }
}