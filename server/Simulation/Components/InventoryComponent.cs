using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct InventoryComponent(NTT EntityId, int storageSpace)
{
    public readonly NTT EntityId = EntityId;
    public int TotalCapacity = storageSpace;
    public int Triangles = 0;
    public int Squares = 0;
    public int Pentagons = 0;
    public long ChangedTick = NttWorld.Tick;


}