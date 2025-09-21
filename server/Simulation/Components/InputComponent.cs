using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component]
public struct InputComponent(NTT EntityId, Vector2 movement, Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
{
    public readonly NTT EntityId = EntityId;
    public Vector2 MovementAxis = movement;
    public Vector2 MouseDir = mousePos;
    public PlayerInput ButtonStates = buttonState;
    public bool DidBoostLastFrame = false;


}