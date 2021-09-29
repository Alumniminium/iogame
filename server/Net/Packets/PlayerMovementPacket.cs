using System.Runtime.InteropServices;

namespace iogame.Net.Packets
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerMovementPacket
    {
        public Header Header;
        public uint UniqueId;
        public uint TickCounter;
        public bool Up; // todo, replace with a single byte or short and use flags
        public bool Down;
        public bool Left;
        public bool Right;
        public bool Fire;
        public ushort X;
        public ushort Y;

        public static implicit operator PlayerMovementPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(PlayerMovementPacket*)p;
            }
        }
    }
}