using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct SpawnPacket
{
    public Header Header;
    public NTT UniqueId;
    public ShapeType ShapeType;
    public float Rotation;
    public Vector2 Position;
    public uint Color;

    public static SpawnPacket Create(NTT uniqueId, ShapeType shapeType, Vector2 position, float rotation, uint color)
    {
        return new SpawnPacket
        {
            Header = new Header(sizeof(SpawnPacket), PacketId.SpawnPacket),
            UniqueId = uniqueId,
            ShapeType = shapeType,
            Position = position,
            Rotation = rotation,
            Color = color
        };
    }

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