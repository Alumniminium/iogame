using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class ShieldSystem : PixelSystem<ShieldComponent, EnergyComponent>
    {
        public ShieldSystem() : base("Shield System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref ShieldComponent shi, ref EnergyComponent eng)
        {
            var lastCharge = shi.Charge;
            var powerDraw = shi.PowerUse;

            if (shi.Charge < 0)
                shi.Charge = 0;

            if (shi.Charge < shi.MaxCharge)
            {
                shi.Charge += Math.Clamp(shi.RechargeRate * deltaTime, 0, shi.MaxCharge - shi.Charge);
                powerDraw += shi.PowerUseRecharge;
            }

            eng.DiscargeRateAcc += powerDraw;
            if (shi.Charge != lastCharge)
                shi.ChangedTick = Game.CurrentTick;
        }
    }
}