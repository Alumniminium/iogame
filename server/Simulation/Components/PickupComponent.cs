using server.ECS;

namespace server.Simulation.Components;

[Component]
public struct DropResourceComponent(int amount)
{
    public byte Amount = (byte)amount;
}