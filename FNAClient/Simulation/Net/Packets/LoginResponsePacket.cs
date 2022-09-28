using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using RG351MP.Scenes;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct LoginResponsePacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public int MapWidth;
        public int MapHeight;
        public ushort ViewDistance;
        public float PlayerSize;
        public float PlayerDrag;
        public float PlayerElasticity;
        public ushort PlayerMaxSpeed;
        public uint PlayerColor;

        public static implicit operator Memory<byte>(LoginResponsePacket msg)
        {
            var buffer = new byte[sizeof(LoginResponsePacket)];
            fixed (byte* p = buffer)
                *(LoginResponsePacket*)p = *&msg;
            return buffer;
        }

        public static implicit operator LoginResponsePacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(LoginResponsePacket*)p;
        }
    }
}