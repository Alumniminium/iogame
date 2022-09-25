using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace server.Simulation.Net.Packets
{
    [Flags]
    public enum PlayerInput : ushort
    {
        None = 0,
        Thrust = 1,
        InvThrust = 2,
        Left = 4,
        Right = 8,
        Boost = 16,
        RCS = 32,
        Fire = 64,
        Drop = 128,
        Shield = 256,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct PlayerMovementPacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public PlayerInput Inputs;
        public Vector2 MousePosition;

        public static PlayerMovementPacket Create(int UniqueId, uint TickCounter, PlayerInput Inputs, Vector2 MousePosition)
        {
            return new PlayerMovementPacket
            {
                Header = new Header((ushort)sizeof(PlayerMovementPacket), PacketId.PlayerMovePacket),
                UniqueId = UniqueId,
                TickCounter = TickCounter,
                Inputs = Inputs,
                MousePosition = MousePosition
            };
        }

        public static implicit operator Memory<byte>(PlayerMovementPacket msg)
        {
            var buffer = new byte[sizeof(PlayerMovementPacket)];
            fixed (byte* p = buffer)
                *(PlayerMovementPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator PlayerMovementPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(PlayerMovementPacket*)p;
            }
        }
    }
}