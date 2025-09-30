using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Engine, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EngineComponent(float maxThrustNewtons)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public float PowerUse = maxThrustNewtons * 0.01f;
    public float Throttle = 0;
    public float MaxThrustNewtons = maxThrustNewtons;
    public bool RCS = true;
    public float Rotation = 0;
}