using System;
using System.Numerics;
using server.Enums;

namespace server.Simulation.Net;

public unsafe ref struct MovementPacket
{
    public Header Header;
    public int UniqueId;
    public uint TickCounter;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;

    public static MovementPacket Create(int uniqueId, uint tickCounter, Vector2 position, Vector2 velocity, float rotation)
    {
        return new MovementPacket
        {
            Header = new Header(sizeof(MovementPacket), PacketId.MovePacket),
            UniqueId = uniqueId,
            TickCounter = tickCounter,
            Position = position,
            Velocity = velocity,
            Rotation = rotation
        };
    }

    public static implicit operator Memory<byte>(MovementPacket msg)
    {
        var buffer = new byte[sizeof(MovementPacket)];
        fixed (byte* p = buffer)
            *(MovementPacket*)p = *&msg;
        return buffer;
    }
    public static implicit operator MovementPacket(Memory<byte> buffer)
    {
        fixed (byte* p = buffer.Span)
        {
            return *(MovementPacket*)p;
        }
    }
}