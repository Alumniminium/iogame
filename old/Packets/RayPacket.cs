using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct RayPacket
    {
        public Header Header;
        public int UniqueId;
        public int TargetUniqueId;
        public Vector2 Origin;
        public Vector2 Hit;

        public static implicit operator Memory<byte>(RayPacket msg)
        {
            var buffer = new byte[sizeof(RayPacket)];
            fixed (byte* p = buffer)
                *(RayPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator RayPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(RayPacket*)p;
        }
    }
}