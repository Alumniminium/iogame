using System;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class StatusPacket
{
    public NTT UniqueId { get; set; }
    public double Value { get; set; }
    public StatusType Type { get; set; }

    public static Memory<byte> Create(NTT uid, uint val, StatusType type)
    {
        using var writer = new PacketWriter(PacketId.StatusPacket);
        writer.WriteNtt(uid)
              .WriteDouble(val)
              .WriteEnum(type);
        return writer.Finalize();
    }

    public static Memory<byte> Create(NTT uid, double val, StatusType type)
    {
        using var writer = new PacketWriter(PacketId.StatusPacket);
        writer.WriteNtt(uid)
              .WriteDouble(val)
              .WriteEnum(type);
        return writer.Finalize();
    }

    public static Memory<byte> Create(NTT uid, float val, StatusType type)
    {
        using var writer = new PacketWriter(PacketId.StatusPacket);
        writer.WriteNtt(uid)
              .WriteDouble(val)
              .WriteEnum(type);
        return writer.Finalize();
    }

    public static Memory<byte> CreateDespawn(NTT nttId)
    {
        using var writer = new PacketWriter(PacketId.StatusPacket);
        writer.WriteNtt(nttId)
              .WriteDouble(0.0)
              .WriteEnum(StatusType.Alive);
        return writer.Finalize();
    }

    public static StatusPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new StatusPacket
        {
            UniqueId = reader.ReadNtt(),
            Value = reader.ReadDouble(),
            Type = reader.ReadEnum<StatusType>()
        };
    }
}