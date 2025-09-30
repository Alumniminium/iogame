using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

/// <summary>
/// Component that makes an entity a source of gravitational force affecting nearby objects.
/// </summary>
[Component(ComponentType = ComponentType.Gravity, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GravityComponent(float strength, float radius)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = 0;

    /// <summary>
    /// Gravitational acceleration in m/s². Positive values attract, negative values repel.
    /// </summary>
    public float Strength = strength;

    /// <summary>
    /// Maximum radius of gravitational influence in meters.
    /// </summary>
    public float Radius = radius;
}