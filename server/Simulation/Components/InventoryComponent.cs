using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct InventoryComponent
    {
        public int TotalCapacity;
        public int Triangles;
        public int Squares;
        public int Pentagons;
        public uint ChangedTick;

        public InventoryComponent(int storageSpace)
        {
            TotalCapacity = storageSpace;
            Triangles = 0;
            Squares = 0;
            Pentagons = 0;
            ChangedTick = Game.CurrentTick;
        }
    }
}