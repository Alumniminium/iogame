using System.Numerics;
using iogame.Simulation;

namespace iogame.Net.Packets
{
    public unsafe struct MovementPacket
    {
        public Header Header;
        public uint UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public Vector2 Velocity;

        public static MovementPacket Create(uint uniqueId, Vector2 position, Vector2 velocity)
        {
            return new MovementPacket
            {
                Header = new Header(sizeof(MovementPacket), 1005),
                UniqueId = uniqueId,
                Position = position,
                Velocity = velocity,
                TickCounter = Game.TickCount
            };
        }

        public static implicit operator byte[](MovementPacket msg)
        {
            var buffer = new byte[sizeof(MovementPacket)];
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