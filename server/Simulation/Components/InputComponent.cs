using System.Numerics;
using Packets.Enums;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct InputComponent
    {        
        public readonly int EntityId;
        public Vector2 MovementAxis;
        public Vector2 MouseDir;
        public PlayerInput ButtonStates;
        public bool DidBoostLastFrame;

        public InputComponent(int entityId, Vector2 movement, Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
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