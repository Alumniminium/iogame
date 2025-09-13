using System.Numerics;
using System.Runtime.InteropServices;
using Packets.Enums;

namespace Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct LineSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public int TargetUniqueId;
        public Vector2 Origin;
        public Vector2 Hit;

        public static LineSpawnPacket Create(int uniqueId, int targetUniqueId, Vector2 origin, Vector2 hit)
        {
            return new LineSpawnPacket
            {
                Header = new Header(sizeof(LineSpawnPacket), PacketId.LineSpawnPacket),
                UniqueId = uniqueId,
                TargetUniqueId = targetUniqueId,
                Origin = origin,
                Hit = hit
            };
        }

        public static implicit operator Memory<byte>(LineSpawnPacket msg)
        {
            var buffer = new byte[sizeof(LineSpawnPacket)];
            fixed (byte* p = buffer)
                *(LineSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator LineSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(LineSpawnPacket*)p;
        }
    }
}