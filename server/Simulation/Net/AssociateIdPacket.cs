using System;
using System.Runtime.InteropServices;
using System.Text;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct AssociateIdPacket
{
    public Header Header;
    public NTT UniqueId;
    public fixed byte Name[17];

    public string GetName()
    {
        var len = Name[0];
        var txtBytes = new byte[len];
        for (var i = 0; i < txtBytes.Length; i++)
            txtBytes[i] = Name[1 + i];
        return Encoding.ASCII.GetString(txtBytes);
    }

    public static implicit operator Memory<byte>(AssociateIdPacket msg)
    {
        var buffer = new byte[sizeof(AssociateIdPacket)];
        fixed (byte* p = buffer)
            *(AssociateIdPacket*)p = *&msg;
        return buffer;
    }

    public static implicit operator AssociateIdPacket(Memory<byte> buffer)
    {
        fixed (byte* p = buffer.Span)
            return *(AssociateIdPacket*)p;
    }

    public static Memory<byte> Create(NTT uniqueId, string name)
    {
        var packet = new AssociateIdPacket
        {
            Header = new Header(sizeof(AssociateIdPacket), PacketId.AssociateId),
            UniqueId = uniqueId,
        };
        var nameBytes = Encoding.ASCII.GetBytes(name);
        packet.Name[0] = (byte)nameBytes.Length;
        for (var i = 0; i < nameBytes.Length && i < 16; i++)
            packet.Name[1 + i] = nameBytes[i];
        return packet;
    }
}