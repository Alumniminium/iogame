using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct ResourceSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort ResourceId;
        public float Direction;
        public Vector2 Position;
        // public Vector2 Velocity; 


        public static implicit operator Memory<byte>(ResourceSpawnPacket msg)
        {
            var buffer = new byte[sizeof(ResourceSpawnPacket)];
            fixed (byte* p = buffer)
                *(ResourceSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator ResourceSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(ResourceSpawnPacket*)p;
            }
        }
    }
}