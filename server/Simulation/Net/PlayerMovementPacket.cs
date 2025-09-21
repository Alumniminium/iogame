using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct PlayerMovementPacket
{
    public Header Header;
    public NTT UniqueId;
    public uint TickCounter;
    public PlayerInput Inputs;
    public Vector2 MousePosition;

    public static PlayerMovementPacket Create(NTT uniqueId, uint tickCounter, PlayerInput inputs, Vector2 mousePosition)
    {
        return new PlayerMovementPacket
        {
            Header = new Header(sizeof(PlayerMovementPacket), PacketId.InputPacket),
            UniqueId = uniqueId,
            TickCounter = tickCounter,
            Inputs = inputs,
            MousePosition = mousePosition
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