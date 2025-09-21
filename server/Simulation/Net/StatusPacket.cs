using System;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct StatusPacket
{
    public Header Header;
    public NTT UniqueId;
    public double Value;
    public StatusType Type;

    public static StatusPacket Create(NTT uid, uint val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket Create(NTT uid, double val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket Create(NTT uid, float val, StatusType type)
    {
        return new StatusPacket
        {
            Header = new Header(sizeof(StatusPacket), PacketId.StatusPacket),
            UniqueId = uid,
            Value = val,
            Type = type
        };
    }
    public static StatusPacket CreateDespawn(NTT nttId)
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