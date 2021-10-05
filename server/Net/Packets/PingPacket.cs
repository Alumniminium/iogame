using System.Numerics;
using iogame.Simulation;

namespace iogame.Net.Packets
{
    public unsafe struct PingPacket
    {
        public Header Header;
        public ushort Ping;
        public long TickCounter;

        public static PingPacket Create()
        {
            return new PingPacket
            {
                Header = new Header(sizeof(PingPacket), 9000),
                TickCounter = DateTime.UtcNow.Ticks,
                Ping = 0
            };
        }

        public static implicit operator byte[](PingPacket msg)
        {
            var buffer = new byte[sizeof(PingPacket)];
            fixed (byte* p = buffer)
                *(PingPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator PingPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(PingPacket*)p;
            }
        }
    }
}