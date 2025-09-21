using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct LoginResponsePacket
{
    public Header Header;
    public NTT UniqueId;
    public uint TickCounter;
    public Vector2 Position;
    public int MapWidth;
    public int MapHeight;
    public ushort ViewDistance;
    public uint PlayerColor;

    public static LoginResponsePacket Create(NTT uniqueId, long tickCounter, Vector2 position, int mapWidth, int mapHeight, ushort viewDistance, uint playerColor)
    {
        return new LoginResponsePacket
        {
            Header = new Header(sizeof(LoginResponsePacket), PacketId.LoginResponse),
            UniqueId = uniqueId,
            TickCounter = (uint)tickCounter,
            Position = position,
            MapWidth = mapWidth,
            MapHeight = mapHeight,
            ViewDistance = viewDistance,
            PlayerColor = playerColor
        };
    }

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