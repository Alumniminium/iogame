using System;
using server.Enums;

namespace server.Simulation.Net;

public class LoginRequestPacket
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public static Memory<byte> Create(string name)
    {
        using var writer = new PacketWriter(PacketId.LoginRequest);
        writer.WriteString8(name.Length > 16 ? name[..16] : name)
              .WriteString8(string.Empty); // Empty password for backward compatibility
        return writer.Finalize();
    }

    public static LoginRequestPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new LoginRequestPacket
        {
            Username = reader.ReadString8(),
            Password = reader.ReadString8()
        };
    }
}