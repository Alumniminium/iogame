using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct DropResourceComponent
    {
        public readonly int EntityId;
        public byte Amount;

        public DropResourceComponent(int entityId, int amount)
        {
            EntityId = entityId;
            Amount = (byte)amount;
        }
        public override int GetHashCode() => EntityId;
    }
}