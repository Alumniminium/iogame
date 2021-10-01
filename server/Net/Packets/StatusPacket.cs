namespace iogame.Net.Packets
{
    public enum StatusType{
        Alive = 0,
        Health = 1
    }
    public unsafe struct StatusPacket
    {
        public Header Header;
        public uint UniqueId;
        public ulong Value;
        public StatusType Type;

        public static StatusPacket Create(uint uid, ulong val, StatusType type)
        {
            return new StatusPacket
            {
                Header = new Header(sizeof(StatusPacket), 1010),
                UniqueId = uid,
                Value = val,
                Type = type
            };
        }

        public static implicit operator byte[](StatusPacket msg)
        {
            var buffer = new byte[sizeof(StatusPacket)];
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