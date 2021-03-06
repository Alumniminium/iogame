using System.Buffers;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    public unsafe struct MovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public float Rotation;

        public static MovementPacket Create(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            return new MovementPacket
            {
                Header = new Header(sizeof(MovementPacket), 1005),
                UniqueId = ntt.Id,
                Position = phy.Position,
                Rotation = phy.RotationRadians,
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