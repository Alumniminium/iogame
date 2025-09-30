using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

/// <summary>
/// Engine component controlling ship propulsion, throttle, and RCS state.
/// Manages thrust force generation and energy consumption.
/// </summary>
[Component(ComponentType = ComponentType.Engine, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EngineComponent(float maxThrustNewtons)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    /// <summary>Energy consumption rate based on thrust capacity</summary>
    public float PowerUse = maxThrustNewtons * 0.01f;
    /// <summary>Current throttle setting (0 to 1)</summary>
    public float Throttle = 0;
    /// <summary>Maximum thrust force in Newtons</summary>
    public float MaxThrustNewtons = maxThrustNewtons;
    /// <summary>Whether RCS (Reaction Control System) is active for dampening</summary>
    public bool RCS = true;
}