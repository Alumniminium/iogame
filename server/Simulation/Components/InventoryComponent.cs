using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct InventoryComponent(int entityId, int storageSpace)
{
    public readonly int EntityId = entityId;
    public int TotalCapacity = storageSpace;
    public int Triangles = 0;
    public int Squares = 0;
    public int Pentagons = 0;
    public uint ChangedTick = Game.CurrentTick;

    public override int GetHashCode() => EntityId;
}