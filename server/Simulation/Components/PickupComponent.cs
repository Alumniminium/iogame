using System.Security.Principal;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct PickupComponent
    {
        public byte Id; // equal to sides i guess, 3 for triangle, 5 for pentagon
        public byte Amount;

        public PickupComponent(int id, int amount)
        {
            Id=(byte)id;
            Amount=(byte)amount;
        }
    }

    [Component]
    public struct InventoryComponent
    {
        public int TotalStorageSpace;
        public int Triangles;
        public int Squares;
        public int Pentagons;

        public InventoryComponent(int storageSpace)
        {
            TotalStorageSpace = storageSpace;
            Triangles = 0;
            Squares=0;
            Pentagons = 0;
        }
    }
}