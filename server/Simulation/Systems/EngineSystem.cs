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
                phy.Rotation -= 0.02f;
            if (inp.ButtonStates.HasFlag(ButtonState.Right))
                phy.Rotation += 0.02f;

            if (inp.ButtonStates.HasFlags(ButtonState.Thrust))
                direction = phy.Forward;
            if (inp.ButtonStates.HasFlags(ButtonState.InvThrust))
                direction = -phy.Forward;
            
            var propulsion = direction * eng.MaxPropulsion;
            phy.Acceleration = propulsion * deltaTime;
        }
    }
}