using System.Numerics;
using Packets.Enums;

namespace Packets
{
    public unsafe ref struct MovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public float Rotation;

        public static MovementPacket Create(int uniqueId, uint tickCounter, Vector2 position, float rotation)
        {
            return new MovementPacket
            {
                Header = new Header(sizeof(MovementPacket), PacketId.MovePacket),
                UniqueId = uniqueId,
                TickCounter = tickCounter,
                Position = position,
                Rotation = rotation
            };
        }

        public static implicit operator Memory<byte>(MovementPacket msg)
        {
            var buffer = new byte[sizeof(MovementPacket)];
            fixed (byte* p = buffer)
                *(MovementPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator MovementPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(MovementPacket*)p;
            }
        }
    }
}