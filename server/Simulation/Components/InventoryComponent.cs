using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Inventory, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InventoryComponent(int storageSpace)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;

    public int TotalCapacity = storageSpace;
    public int Triangles = 0;
    public int Squares = 0;
    public int Pentagons = 0;
}