using System;
using System.Text;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class AssociateIdPacket
{
    public NTT UniqueId { get; set; }
    public string Name { get; set; } = string.Empty;

    public static Memory<byte> Create(NTT uniqueId, string name)
    {
        using var writer = new PacketWriter(PacketId.AssociateId);
        writer.WriteNtt(uniqueId)
              .WriteString8(name.Length > 16 ? name.Substring(0, 16) : name);
        return writer.Finalize();
    }

    public static AssociateIdPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new AssociateIdPacket
        {
            UniqueId = reader.ReadNtt(),
            Name = reader.ReadString8()
        };
    }
}