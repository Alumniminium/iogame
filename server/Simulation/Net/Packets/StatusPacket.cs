using System.Buffers;

namespace server.Simulation.Net.Packets
{
    public enum StatusType : byte
    {
        Alive = 0,
        Health = 1,
        Size = 3,
        Direction = 4,
        InventoryCapacity = 100,
        InventoryTriangles = 101,
        InventorySquares = 102,
        InventoryPentagons = 103,
    }
    public unsafe struct StatusPacket
    {
        public Header Header;
        public int UniqueId;
        public uint Value;
        public StatusType Type;

        public static StatusPacket Create(int uid, uint val, StatusType type)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = uid,
                Value = val,
                Type = type
            };
        }
        public static StatusPacket CreateDespawn(int nttId)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = nttId,
                Type = StatusType.Alive,
                Value = 0
            };
        }


        public static implicit operator byte[](StatusPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(StatusPacket));
            fixed (byte* p = buffer)
                *(StatusPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator StatusPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(StatusPacket*) p;
        }
    }
}