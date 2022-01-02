using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class EngineSystem : PixelSystem<PhysicsComponent, InputComponent, EngineComponent>
    {
        public EngineSystem() : base("Engine System", 12) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref InputComponent inp, ref EngineComponent eng)
        {
            var turnDirection = 0f;

            if (inp.ButtonStates.HasFlag(ButtonState.Left))
                turnDirection -= 1f;
            else if (inp.ButtonStates.HasFlag(ButtonState.Right))
                turnDirection = 1f;
            if (inp.ButtonStates.HasFlag(ButtonState.Boost))
                eng.Throttle = 1;
            else if (inp.ButtonStates.HasFlags(ButtonState.Thrust))
                eng.Throttle = Math.Clamp(eng.Throttle + 1 * deltaTime, -1, 1);
            else if (inp.ButtonStates.HasFlags(ButtonState.InvThrust))
                eng.Throttle = Math.Clamp(eng.Throttle - 1 * deltaTime, -1, 1);

            // FConsole.WriteLine($"Throttle: {eng.Throttle * 100:##.##}%");


            eng.RCS = inp.ButtonStates.HasFlag(ButtonState.RCS);

            var propulsion = phy.Forward * eng.MaxPropulsion * eng.Throttle;

            if (eng.RCS)
            {
                var powerAvailable = 1 - Math.Abs(eng.Throttle);

                phy.AngularVelocity *= 0.9f;

                if (phy.Velocity != Vector2.Zero)
                {
                    var deltaDir = phy.Forward - Vector2.Normalize(phy.Velocity);
                    var stabilizationPropulsion = deltaDir * eng.MaxPropulsion * 0.25f;
                    stabilizationPropulsion = stabilizationPropulsion.ClampMagnitude(Math.Min(stabilizationPropulsion.Length(), phy.Velocity.Length()));
                    propulsion += stabilizationPropulsion;
                }
            }
            phy.Acceleration = propulsion;
            phy.AngularVelocity += turnDirection * eng.MaxPropulsion * Math.Min(0.25f, 1 - Math.Abs(eng.Throttle));
        }
    }
}