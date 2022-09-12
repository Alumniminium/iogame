using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct DropResourceComponent
    {
        public byte Id; // equal to sides i guess, 3 for triangle, 5 for pentagon
        public byte Amount;

        public DropResourceComponent(int id, int amount)
        {
            Id = (byte)id;
            Amount = (byte)amount;
        }
    }
}