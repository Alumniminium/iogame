using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Flags]
    public enum ButtonState : byte
    {
        None =      0b_00000000,
        Thrust =    0b_00000001,
        InvThrust = 0b_00000010,
        Left =      0b_00000100,
        Right =     0b_00001000,
        Boost =     0b_00010000,
        RCS =       0b_00100000,
        Fire =      0b_01000000,
    }
    [Component]
    public struct InputComponent
    {
        public Vector2 MovementAxis;
        public Vector2 MousePositionWorld;
        public ButtonState ButtonStates;

        public InputComponent(Vector2 movement, Vector2 mousePos, ButtonState buttonState = ButtonState.None)
        {
            MovementAxis = movement;
            MousePositionWorld = mousePos;
            ButtonStates = buttonState;
        }
    }
}