using System;
using System.Runtime.InteropServices;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct RequestSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public int EntityId;

        public static implicit operator Memory<byte>(RequestSpawnPacket msg)
        {
            var buffer = new byte[sizeof(RequestSpawnPacket)];
            fixed (byte* p = buffer)
                *(RequestSpawnPacket*)p = *&msg;
            return buffer;
        }

        public static implicit operator RequestSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(RequestSpawnPacket*)p;
            }
        }
    }
}