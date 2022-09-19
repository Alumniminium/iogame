using System;

namespace server.Simulation.Net.Packets
{
    public unsafe ref struct PingPacket
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

        public static implicit operator Memory<byte>(PingPacket msg)
        {
            var buffer = new byte[sizeof(PingPacket)];
            fixed (byte* p = buffer)
                *(PingPacket*)p = *&msg;
            return buffer;
        }

        public static implicit operator PingPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(PingPacket*)p;
        }
    }
}