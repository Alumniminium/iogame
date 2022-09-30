using System.Numerics;
using System.Runtime.InteropServices;
using Packets.Enums;

namespace Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct SpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ShapeType ShapeType;
        public float Width;
        public float Height;
        public float Direction;
        public Vector2 Position;
        public uint Color;

        public static SpawnPacket Create(int uniqueId, ShapeType shapeType, float radius, float width, float height, Vector2 position, float rotation, uint color)
        {
            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), PacketId.CustomSpawnPacket),
                UniqueId = uniqueId,
                ShapeType = shapeType,
                Width = shapeType == ShapeType.Circle ? radius : width,
                Height =shapeType == ShapeType.Circle ? radius : height,
                Position = position,
                Direction = rotation,
                Color = color
            };
        }

        public static implicit operator Memory<byte>(SpawnPacket msg)
        {
            var buffer = new byte[sizeof(SpawnPacket)];
            fixed (byte* p = buffer)
                *(SpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator SpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(SpawnPacket*)p;
        }
    }
}