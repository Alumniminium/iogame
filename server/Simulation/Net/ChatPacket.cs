using System;
using System.Text;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class ChatPacket
{
    public NTT UserId { get; set; }
    public byte Channel { get; set; }
    public string Message { get; set; } = string.Empty;

    public static Memory<byte> Create(NTT id, string text, byte channel = 0)
    {
        using var writer = new PacketWriter(PacketId.ChatPacket);
        writer.WriteNtt(id)
              .WriteByte(channel)
              .WriteString8(text.Length > 255 ? text.Substring(0, 255) : text);
        return writer.Finalize();
    }

    public static ChatPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new ChatPacket
        {
            UserId = reader.ReadNtt(),
            Channel = reader.ReadByte(),
            Message = reader.ReadString8()
        };
    }
}