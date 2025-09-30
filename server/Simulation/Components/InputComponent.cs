using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Input, NetworkSync = true)]
public struct InputComponent(Vector2 movement, Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
{
    public long ChangedTick = NttWorld.Tick;
    public Vector2 MovementAxis = movement;
    public Vector2 MouseDir = mousePos;
    public PlayerInput ButtonStates = buttonState;
    public bool DidBoostLastFrame = false;
}