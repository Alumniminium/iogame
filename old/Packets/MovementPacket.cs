using System;
using Microsoft.Xna.Framework;
using RG351MP.Scenes;

namespace server.Simulation.Net.Packets
{
    public unsafe ref struct MovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public float Rotation;

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