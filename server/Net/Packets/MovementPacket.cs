using System.Buffers;
using System.Numerics;
using iogame.Simulation;

namespace iogame.Net.Packets
{
    public unsafe struct MovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public Vector2 Velocity;

        public static MovementPacket Create(int uniqueId, in Vector2 position, in Vector2 velocity)
        {
            return new MovementPacket
            {
                Header = new Header(sizeof(MovementPacket), 1005),
                UniqueId = uniqueId,
                Position = position,
                Velocity = velocity,
                TickCounter = Game.CurrentTick
            };
        }

        public static implicit operator byte[](MovementPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MovementPacket));
            fixed (byte* p = buffer)
                *(MovementPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator MovementPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(MovementPacket*)p;
            }
        }
    }
}