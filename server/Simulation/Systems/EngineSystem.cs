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
            var direction = Vector2.Zero;

            if (inp.ButtonStates.HasFlag(ButtonState.Left))
                phy.AngularVelocity -= 0.14f;
            if (inp.ButtonStates.HasFlag(ButtonState.Right))
                phy.AngularVelocity += 0.14f;
            if (inp.ButtonStates.HasFlags(ButtonState.Thrust))
                direction = phy.Forward;
            if (inp.ButtonStates.HasFlags(ButtonState.InvThrust))
                direction = -phy.Forward;

            eng.RCS = inp.ButtonStates.HasFlag(ButtonState.RCS);

            var propulsion = direction * eng.MaxPropulsion;

            if (eng.RCS)
            {
                propulsion *= 0.5f;
                
                phy.AngularVelocity *= 0.95f;

                if (phy.Velocity.ClampMagnitude(1) != direction)
                {
                    var deltaDir = direction - phy.Velocity.ClampMagnitude(1);           
                    var stabilizationPropulsion = deltaDir * eng.MaxPropulsion;
                    stabilizationPropulsion = stabilizationPropulsion.ClampMagnitude(Math.Min(eng.MaxPropulsion*0.5f,eng.MaxPropulsion*phy.Velocity.Length()));
                    propulsion += stabilizationPropulsion;
                }
            }

            phy.Acceleration = propulsion * deltaTime;
        }
    }
}