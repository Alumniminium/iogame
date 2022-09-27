using System;
using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Flags]
    public enum ButtonState : ushort
    {
        None = 0,
        Thrust = 1,
        InvThrust = 2,
        Left = 4,
        Right = 8,
        Boost = 16,
        RCS = 32,
        Fire = 64,
        Drop = 128,
        Shield = 256,
    }
    [Component]
    public struct InputComponent
    {        
        public readonly int EntityId;
        public Vector2 MovementAxis;
        public Vector2 MouseDir;
        public ButtonState ButtonStates;
        public bool DidBoostLastFrame;

        public InputComponent(int entityId, Vector2 movement, Vector2 mousePos, ButtonState buttonState = ButtonState.None)
        {
            EntityId = entityId;
            MovementAxis = movement;
            MouseDir = mousePos;
            ButtonStates = buttonState;
            DidBoostLastFrame = false;
        }
        public override int GetHashCode() => EntityId;
    }
}