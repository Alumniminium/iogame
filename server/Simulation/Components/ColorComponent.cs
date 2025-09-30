using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Color, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ColorComponent(uint color)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public uint Color = color;
}