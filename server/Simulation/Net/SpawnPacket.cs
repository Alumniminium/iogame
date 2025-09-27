using System;
using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class SpawnPacket
{
    public NTT UniqueId { get; set; }
    public ShapeType ShapeType { get; set; }
    public float Rotation { get; set; }
    public Vector2 Position { get; set; }
    public uint Color { get; set; }
    public List<ShipPart> Parts { get; set; }

    public SpawnPacket()
    {
        Parts = new List<ShipPart>();
    }

    public static SpawnPacket Create(NTT uniqueId, ShapeType shapeType, Vector2 position, float rotation, uint color, List<ShipPart> parts)
    {
        return new SpawnPacket
        {
            UniqueId = uniqueId,
            ShapeType = shapeType,
            Position = position,
            Rotation = rotation,
            Color = color,
            Parts = parts ?? new List<ShipPart>(),
        };
    }

    public Memory<byte> ToBuffer()
    {
        using var writer = new PacketWriter(PacketId.SpawnPacket);
        writer.WriteNtt(UniqueId);
        writer.WriteInt32((int)ShapeType);
        writer.WriteFloat(Rotation);
        writer.WriteFloat(Position.X);
        writer.WriteFloat(Position.Y);
        writer.WriteUInt32(Color);

        // Write part count
        writer.WriteInt16((short)Parts.Count);

        // Write each part
        foreach (var part in Parts)
        {
            writer.WriteSByte(part.GridX);
            writer.WriteSByte(part.GridY);
            writer.WriteByte(part.Type);
            writer.WriteByte(part.Shape);
            writer.WriteByte(part.Rotation);
        }

        return writer.Finalize();
    }

    public static SpawnPacket FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var packet = new SpawnPacket();

        // Read basic spawn data
        packet.UniqueId = reader.ReadNtt();
        packet.ShapeType = (ShapeType)reader.ReadInt32();
        packet.Rotation = reader.ReadFloat();
        packet.Position = new Vector2(reader.ReadFloat(), reader.ReadFloat());
        packet.Color = reader.ReadUInt32();

        // Read part count
        var partCount = reader.ReadInt16();

        // Read each part
        for (int i = 0; i < partCount; i++)
        {
            var part = new ShipPart(
                reader.ReadSByte(), // gridX
                reader.ReadSByte(), // gridY
                reader.ReadByte(), // type
                reader.ReadByte(), // shape
                reader.ReadByte()  // rotation
            );
            packet.Parts.Add(part);
        }

        return packet;
    }
}