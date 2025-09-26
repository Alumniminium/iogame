using System;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class RequestSpawnPacket
{
    public NTT Requester { get; set; }
    public NTT Target { get; set; }

    public static Memory<byte> Create(NTT requester, NTT target)
    {
        using var writer = new PacketWriter(PacketId.RequestSpawnPacket);
        writer.WriteNtt(requester)
              .WriteNtt(target);
        return writer.Finalize();
    }

    public static RequestSpawnPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var header = reader.ReadHeader(); // Skip header

        return new RequestSpawnPacket
        {
            Requester = reader.ReadNtt(),
            Target = reader.ReadNtt()
        };
    }
}