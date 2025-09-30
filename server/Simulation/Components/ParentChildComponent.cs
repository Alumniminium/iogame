using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.ParentChild, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ParentChildComponent(NTT parentId, sbyte gridX = 0, sbyte gridY = 0, byte shape = 0, byte rotation = 0)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    /// <summary>
    /// Entity ID of the parent entity
    /// </summary>
    public NTT ParentId = parentId;

    /// <summary>Grid X position in ship builder grid</summary>
    public sbyte GridX = gridX;
    /// <summary>Grid Y position in ship builder grid</summary>
    public sbyte GridY = gridY;
    /// <summary>Shape type (1=triangle, 2=box, etc.)</summary>
    public byte Shape = shape;
    /// <summary>Rotation in 90° increments (0-3 = 0°, 90°, 180°, 270°)</summary>
    public byte Rotation = rotation;
}