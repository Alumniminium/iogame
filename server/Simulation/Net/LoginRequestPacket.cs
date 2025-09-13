using System;
using System.Text;
using server.Enums;

namespace server.Simulation.Net;

public unsafe ref struct LoginRequestPacket
{
    public Header Header;
    public fixed byte Username[17];
    public fixed byte Password[17];

    public string GetUsername()
    {
        var len = Username[0];
        var txtBytes = new byte[len];
        for (var i = 0; i < txtBytes.Length; i++)
            txtBytes[i] = Username[1 + i];
        return Encoding.ASCII.GetString(txtBytes);
    }
    public string GetPassword()
    {
        var len = Password[0];
        var txtBytes = new byte[len];
        for (var i = 0; i < txtBytes.Length; i++)
            txtBytes[i] = Password[1 + i];
        return Encoding.ASCII.GetString(txtBytes);
    }

    public static LoginRequestPacket Create(string name)
    {
        var packet = new LoginRequestPacket
        {
            Header = new Header(sizeof(LoginRequestPacket), PacketId.LoginRequest)
        };
        var nameBytes = Encoding.ASCII.GetBytes(name);
        packet.Username[0] = (byte)nameBytes.Length;
        for (var i = 0; i < nameBytes.Length; i++)
            packet.Username[1 + i] = nameBytes[i];
        return packet;
    }

    public static implicit operator Memory<byte>(LoginRequestPacket msg)
    {
        var buffer = new byte[sizeof(LoginRequestPacket)];
        fixed (byte* p = buffer)
            *(LoginRequestPacket*)p = *&msg;
        return buffer;
    }
    public static implicit operator LoginRequestPacket(Memory<byte> buffer)
    {
        fixed (byte* p = buffer.Span)
            return *(LoginRequestPacket*)p;
    }
}