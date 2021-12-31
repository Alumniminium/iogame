using System.Numerics;
using System.Runtime.InteropServices;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerMovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public ButtonState Inputs;
        public Vector2 MousePosition;

        public static implicit operator PlayerMovementPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(PlayerMovementPacket*)p;
        }
    }
}