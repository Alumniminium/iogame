using System.Buffers;
using iogame.ECS;

namespace iogame.Net.Packets
{
    public enum StatusType{
        Alive = 0,
        Health = 1
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
        public static StatusPacket CreateDespawn(int entityId)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = entityId,
                Type = StatusType.Alive
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
            {
                return *(StatusPacket*)p;
            }
        }
    }
}