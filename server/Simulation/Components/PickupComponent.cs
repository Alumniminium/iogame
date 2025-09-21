using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DropResourceComponent(NTT EntityId, int amount)
{
    public readonly NTT EntityId = EntityId;
    public byte Amount = (byte)amount;


}