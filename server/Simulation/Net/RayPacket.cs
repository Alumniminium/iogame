using System;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class RayPacket
{
    public NTT UniqueId { get; set; }
    public NTT TargetUniqueId { get; set; }
    public Vector2 Origin { get; set; }
    public Vector2 Hit { get; set; }

    public static Memory<byte> Create(NTT uniqueId, NTT targetUniqueId, Vector2 origin, Vector2 hit)
    {
        using var writer = new PacketWriter(PacketId.LineSpawnPacket);
        writer.WriteNtt(uniqueId)
              .WriteNtt(targetUniqueId)
              .WriteVector2(origin)
              .WriteVector2(hit);
        return writer.Finalize();
    }

    public static RayPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new RayPacket
        {
            UniqueId = reader.ReadNtt(),
            TargetUniqueId = reader.ReadNtt(),
            Origin = reader.ReadVector2(),
            Hit = reader.ReadVector2()
        };
    }
}