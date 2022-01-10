using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class EngineSystem : PixelSystem<PhysicsComponent, EngineComponent>
    {
        public EngineSystem() : base("Engine System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref EngineComponent eng)
        {
            var propulsion = phy.Forward * (eng.MaxPropulsion * eng.Throttle);

            if (eng.RCS)
            {
                var powerAvailable = eng.MaxPropulsion * (1 - MathF.Abs(eng.Throttle)) / 2;

                var powerToCancelAngVel = Math.Abs(phy.AngularVelocity * 10);
                var powerToCancelDrift = Math.Abs(phy.RotationRadians - Vector2.Normalize(phy.Velocity).ToRadians() * phy.Velocity.Length());

                phy.AngularVelocity *= 1f - Math.Abs(powerToCancelAngVel - powerAvailable);

                phy.Velocity = Vector2.Lerp(phy.Velocity, phy.Forward * powerToCancelDrift * eng.Throttle, deltaTime);
            }
            phy.Acceleration = propulsion;
            phy.AngularVelocity = eng.Rotation * 3;


            var direction = (-phy.Forward).ToRadians();

            var ray = new Ray(phy.Position, direction.ToDegrees());
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();
            for(int i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                ref var bPhy = ref vwp.EntitiesVisible[i].Entity.Get<PhysicsComponent>();
                ref readonly var bShp = ref vwp.EntitiesVisible[i].Entity.Get<ShapeComponent>();
                var rayHit = ray.Cast(bPhy.Position,20);

                if(rayHit == Vector2.Zero || Vector2.Distance(rayHit, phy.Position) > 150)
                    continue;
                    
                bPhy.Velocity += -propulsion;
            }
        }
    }
}