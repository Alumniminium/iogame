using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

/// <summary>
/// Player input component storing mouse direction and button states.
/// Synchronized from client to server for player control.
/// </summary>
[Component(ComponentType = ComponentType.Input, NetworkSync = true)]
public struct InputComponent(Vector2 mousePos, PlayerInput buttonState = PlayerInput.None)
{
    /// <summary>Tick when this component last changed for network sync</summary>
    public long ChangedTick = NttWorld.Tick;
    /// <summary>Mouse direction vector for aiming</summary>
    public Vector2 MouseDir = mousePos;
    /// <summary>Bitmask of currently pressed buttons</summary>
    public PlayerInput ButtonStates = buttonState;
    /// <summary>Internal flag for boost throttle management</summary>
    public bool DidBoostLastFrame = false;
}