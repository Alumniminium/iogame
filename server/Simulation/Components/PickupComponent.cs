using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct DropResourceComponent
    {
        public byte Amount;

        public DropResourceComponent(int amount) => Amount = (byte)amount;
    }
}