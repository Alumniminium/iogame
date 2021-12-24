using System.Buffers;
using System.Numerics;

namespace server.Simulation.Net.Packets
{
    public unsafe struct MovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;

        public static MovementPacket Create(int uniqueId, ref Vector2 position)
        {
            return new MovementPacket
            {
                Header = new Header(sizeof(MovementPacket), 1005),
                UniqueId = uniqueId,
                Position = position,
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