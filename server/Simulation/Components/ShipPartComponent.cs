using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.ShipPart, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShipPartComponent(sbyte gridX, sbyte gridY, byte type, byte shape, byte rotation)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    /// <summary>
    /// Grid position X coordinate (relative to parent ship)
    /// </summary>
    public sbyte GridX = gridX;

    /// <summary>
    /// Grid position Y coordinate (relative to parent ship)
    /// </summary>
    public sbyte GridY = gridY;

    /// <summary>
    /// Part type: 0=hull, 1=shield, 2=engine
    /// </summary>
    public byte Type = type;

    /// <summary>
    /// Part shape: 1=triangle, 2=square
    /// </summary>
    public byte Shape = shape;

    /// <summary>
    /// Part rotation: 0=0°, 1=90°, 2=180°, 3=270°
    /// </summary>
    public byte Rotation = rotation;
}