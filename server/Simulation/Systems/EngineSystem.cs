using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net.Packets;

namespace server.Simulation.Systems
{
    public class EngineSystem : PixelSystem<PhysicsComponent, EngineComponent>
    {
        public EngineSystem() : base("Engine System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent c1, ref EngineComponent c2)
        {
            var propulsion = c1.Forward * (c2.MaxPropulsion * c2.Throttle);

            if (c2.RCS)
            {
                var powerAvailable = c2.MaxPropulsion * (1 - MathF.Abs(c2.Throttle)) / 2;

                var powerToCancelAngVel = Math.Abs(c1.AngularVelocity * 10);
                var powerToCancelDrift = Math.Abs(c1.RotationRadians - Vector2.Normalize(c1.Velocity).ToRadians() * c1.Velocity.Length());

                c1.AngularVelocity *= 1f - Math.Abs(powerToCancelAngVel - powerAvailable);

                c1.Velocity = Vector2.Lerp(c1.Velocity, c1.Forward * powerToCancelDrift * c2.Throttle, deltaTime);
            }
            c1.Acceleration = propulsion;
            c1.AngularVelocity = c2.Rotation * 3;


            var direction = (-c1.Forward).ToRadians();
            var deg = direction.ToDegrees();

            var ray = new Ray(c1.Position, deg + (5 * Random.Shared.Next(-5, 6)));
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                ref var bPhy = ref vwp.EntitiesVisible[i].Get<PhysicsComponent>();
                ref readonly var bShp = ref vwp.EntitiesVisible[i].Get<ShapeComponent>();
                var rayHit = ray.Cast(bPhy.Position, bShp.Size);

                if (rayHit == Vector2.Zero || Vector2.Distance(rayHit, c1.Position) > 50)
                    continue;

                if(ntt.Type == EntityType.Player)
                    ntt.NetSync(RayPacket.Create(in ntt, vwp.EntitiesVisible[i], ref rayHit));

                bPhy.Velocity += -propulsion;
            }
        }
    }
}