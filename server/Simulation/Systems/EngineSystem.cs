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
            var direction = phy.Forward;

            if (inp.ButtonStates.HasFlag(ButtonState.Left))
                phy.AngularVelocity -= 0.14f;
            else if (inp.ButtonStates.HasFlag(ButtonState.Right))
                phy.AngularVelocity += 0.14f;
            if (inp.ButtonStates.HasFlags(ButtonState.InvThrust))
                direction = -direction;
            
            if(inp.ButtonStates.HasFlag(ButtonState.Boost))
                eng.Throttle = 1;
            else if (inp.ButtonStates.HasFlags(ButtonState.Thrust))
                eng.Throttle = Math.Clamp(eng.Throttle + 1 * deltaTime, -1, 1);
            else if (inp.ButtonStates.HasFlags(ButtonState.InvThrust))
                eng.Throttle = Math.Clamp(eng.Throttle - 1 * deltaTime, -1, 1);

            // FConsole.WriteLine($"Throttle: {eng.Throttle * 100:##.##}%");
            

            eng.RCS = inp.ButtonStates.HasFlag(ButtonState.RCS);

            var propulsion = direction * eng.MaxPropulsion * eng.Throttle;

            if (eng.RCS)
            {
                propulsion *= 0.75f;

                phy.AngularVelocity *= 0.9f;

                if (phy.Velocity != Vector2.Zero)
                {
                    var deltaDir = direction - Vector2.Normalize(phy.Velocity);
                    var stabilizationPropulsion = deltaDir * eng.MaxPropulsion * 0.25f;
                    stabilizationPropulsion = stabilizationPropulsion.ClampMagnitude(Math.Min(stabilizationPropulsion.Length(), phy.Velocity.Length()));
                    propulsion += stabilizationPropulsion;
                }
            }
            phy.Acceleration = propulsion;
        }
    }
}