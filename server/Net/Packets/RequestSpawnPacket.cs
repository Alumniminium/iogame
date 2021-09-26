using System.Runtime.InteropServices;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public unsafe struct RequestSpawnPacket
    {
        public Header Header;
        public uint UniqueId;
        public uint EntityId;

        public static implicit operator byte[](RequestSpawnPacket msg)
        {
            var buffer = new byte[sizeof(RequestSpawnPacket)];
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