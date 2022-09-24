using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using server.Simulation.Database;

namespace server.Simulation.Net.Packets
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