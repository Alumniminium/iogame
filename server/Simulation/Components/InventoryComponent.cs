using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct InventoryComponent
    {
        public readonly int EntityId;
        public int TotalCapacity;
        public int Triangles;
        public int Squares;
        public int Pentagons;
        public uint ChangedTick;

        public InventoryComponent(int entityId, int storageSpace)
        {
            EntityId = entityId;
            TotalCapacity = storageSpace;
            Triangles = 0;
            Squares = 0;
            Pentagons = 0;
            ChangedTick = Game.CurrentTick;
        }
        public override int GetHashCode() => EntityId;
    }
}