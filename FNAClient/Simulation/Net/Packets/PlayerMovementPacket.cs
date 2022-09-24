using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace server.Simulation.Net.Packets
{

    // [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // public unsafe ref struct PlayerMovementPacket
    // {
    //     public Header Header;
    //     public int UniqueId;
    //     public uint TickCounter;
    //     public ButtonState Inputs;
    //     public Vector2 MousePosition;

        
    //     public static implicit operator Memory<byte>(PlayerMovementPacket msg)
    //     {
    //         var buffer = new byte[sizeof(PlayerMovementPacket)];
    //         fixed (byte* p = buffer)
    //             *(PlayerMovementPacket*)p = *&msg;
    //         return buffer;
    //     }
    //     public static implicit operator PlayerMovementPacket(Memory<byte> buffer)
    //     {
    //         fixed (byte* p = buffer.Span)
    //         {
    //             return *(PlayerMovementPacket*)p;
    //         }
    //     }
    // }
}