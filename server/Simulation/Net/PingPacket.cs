using System;
using server.Enums;

namespace server.Simulation.Net;

public class PingPacket
{
    public ushort Ping { get; set; }
    public long TickCounter { get; set; }

    public static Memory<byte> Create()
    {
        using var writer = new PacketWriter(PacketId.Ping);
        writer.WriteUInt16(0)
        .WriteInt64(DateTime.UtcNow.Ticks);
        return writer.Finalize();
    }

    public static PingPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new PingPacket
        {
            Ping = reader.ReadUInt16(),
            TickCounter = reader.ReadInt64()
        };
    }
}