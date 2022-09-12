using System.Buffers;
using System.Runtime.InteropServices;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RequestSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public int EntityId;

        public static implicit operator byte[](RequestSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(RequestSpawnPacket));
            fixed (byte* p = buffer)
                *(RequestSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator RequestSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(RequestSpawnPacket*)p;
            }
        }
    }
}