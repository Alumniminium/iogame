using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{

    public sealed class EngineSystem : PixelSystem<PhysicsComponent, EngineComponent, EnergyComponent>
    {
        public EngineSystem() : base("Engine System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref EngineComponent eng, ref EnergyComponent nrg)
        {
            var powerDraw = eng.PowerUse * eng.Throttle;

            if (nrg.AvailableCharge < powerDraw)
            {
                eng.Throttle = nrg.AvailableCharge / eng.PowerUse;
                powerDraw = eng.PowerUse * eng.Throttle;
                eng.ChangedTick = Game.CurrentTick;
            }

            nrg.DiscargeRateAcc += powerDraw;

            var propulsion = phy.Forward * (eng.MaxPropulsion * eng.Throttle);

            if (propulsion == Vector2.Zero && eng.Rotation == 0 && !eng.RCS)
                return;

            phy.Drag = eng.RCS ? 0.01f : 0.001f;

            phy.Acceleration += propulsion;
            phy.AngularVelocity = eng.Rotation * 3;

            if (propulsion == Vector2.Zero)
                return;

            // var direction = (-phy.Forward).ToRadians();
            // var deg = direction.ToDegrees();

            // var ray = new Ray(phy.Position, deg + (5 * Random.Shared.Next(-6, 7)));
            // ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            // for (var i = 0; i < vwp.EntitiesVisible.Length; i++)
            // {
            //     ref readonly var b = ref vwp.EntitiesVisible[i];
            //     ref var bPhy = ref b.Get<PhysicsComponent>();
            //     Vector2 rayHit = default;

            //     if(bPhy.ShapeType == Database.ShapeType.Circle)
            //         rayHit = ray.Cast(bPhy.Position, bPhy.Radius);
            //     else if (bPhy.ShapeType == Database.ShapeType.Box)
            //         rayHit = ray.Cast(new RectangleF(bPhy.Position.X, bPhy.Position.Y, bPhy.Width, bPhy.Height));

            //     if (rayHit == Vector2.Zero || Vector2.Distance(rayHit, phy.Position) > 150)
            //         continue;

            //     if (ntt.Type == EntityType.Player)
            //         ntt.NetSync(RayPacket.Create(in ntt, in b, ref rayHit));

            //     bPhy.Acceleration += -(propulsion / (bPhy.Position - rayHit).LengthSquared()) * deltaTime;
            // }
        }
    }
}