using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DropResourceComponent(int entityId, int amount)
{
    public readonly int EntityId = entityId;
    public byte Amount = (byte)amount;

    public override int GetHashCode() => EntityId;
}