using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public sealed class EngineSystem : PixelSystem<PhysicsComponent, EngineComponent>
    {
        public EngineSystem() : base("Engine System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref EngineComponent eng)
        {
            var propulsion = phy.Forward * (eng.MaxPropulsion * eng.Throttle);

            if(propulsion == Vector2.Zero && eng.Rotation == 0 && !eng.RCS)
                return;

            if (eng.RCS)
            {
                var powerAvailable = eng.MaxPropulsion * (1 - MathF.Abs(eng.Throttle)) / 2;

                var powerToCancelAngVel = Math.Abs(phy.AngularVelocity * 10);
                var powerToCancelDrift = Math.Abs(phy.RotationRadians - Vector2.Normalize(phy.Velocity).ToRadians() * phy.Velocity.Length());
                if(float.IsNaN(powerToCancelDrift))
                    powerToCancelDrift = 0;
                if(float.IsNaN(powerToCancelAngVel))
                    powerToCancelAngVel = 0;
                    
                phy.AngularVelocity *= 1f - Math.Abs(powerToCancelAngVel - powerAvailable);

                phy.Velocity = Vector2.Lerp(phy.Velocity, phy.Forward * powerToCancelDrift * eng.Throttle, deltaTime);
            }
            phy.Acceleration += propulsion;
            phy.AngularVelocity = eng.Rotation * 3;


            var direction = (-phy.Forward).ToRadians();
            var deg = direction.ToDegrees();

            var ray = new Ray(phy.Position, deg + (5 * Random.Shared.Next(-5, 6)));
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                ref var bPhy = ref vwp.EntitiesVisible[i].Get<PhysicsComponent>();
                ref readonly var bShp = ref vwp.EntitiesVisible[i].Get<ShapeComponent>();
                var rayHit = ray.Cast(bPhy.Position, bShp.Size);

                if (rayHit == Vector2.Zero || Vector2.Distance(rayHit, phy.Position) > 50)
                    continue;

                if (ntt.Type == EntityType.Player)
                    ntt.NetSync(RayPacket.Create(in ntt, vwp.EntitiesVisible[i], ref rayHit));

                bPhy.Velocity += -propulsion;
            }
        }
    }
}