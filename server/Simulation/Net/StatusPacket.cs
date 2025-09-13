using System;
using server.Enums;

namespace server.Simulation.Net;

public unsafe ref struct StatusPacket
{
    public Header Header;
    public int UniqueId;
    public double Value;
    public StatusType Type;

    public static StatusPacket Create(int uid, uint val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket Create(int uid, double val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket Create(int uid, float val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket CreateDespawn(int nttId)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = nttId,
            Type = StatusType.Alive,
            Value = 0
        };
    }


    public static implicit operator Memory<byte>(StatusPacket msg)
    {
        var buffer = new byte[sizeof(StatusPacket)];
        fixed (byte* p = buffer)
            *(StatusPacket*)p = *&msg;
        return buffer;
    }
    public static implicit operator StatusPacket(Memory<byte> buffer)
    {
        fixed (byte* p = buffer.Span)
            return *(StatusPacket*)p;
    }
}