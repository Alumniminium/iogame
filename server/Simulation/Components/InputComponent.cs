using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component]
public struct InputComponent(int entityId, Vector2 movement, Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
{
    public readonly int EntityId = entityId;
    public Vector2 MovementAxis = movement;
    public Vector2 MouseDir = mousePos;
    public PlayerInput ButtonStates = buttonState;
    public bool DidBoostLastFrame = false;

    public override int GetHashCode() => EntityId;
}