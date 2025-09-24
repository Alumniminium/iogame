using System;
using System.Collections.Generic;
using server.ECS;

namespace server.Simulation.Net;

public struct ShipPart
{
    public sbyte GridX; // Changed to signed byte for negative coordinates
    public sbyte GridY; // Changed to signed byte for negative coordinates
    public byte Type; // 0=hull, 1=shield, 2=engine
    public byte Shape; // 1=triangle, 2=square
    public byte Rotation; // 0=0°, 1=90°, 2=180°, 3=270°

    public ShipPart(sbyte gridX, sbyte gridY, byte type, byte shape, byte rotation)
    {
        GridX = gridX;
        GridY = gridY;
        Type = type;
        Shape = shape;
        Rotation = rotation;
    }
}

public class ShipConfigurationPacket
{
    public NTT PlayerId { get; set; }
    public List<ShipPart> Parts { get; set; }
    public sbyte CenterX { get; set; }
    public sbyte CenterY { get; set; }

    public ShipConfigurationPacket()
    {
        Parts = new List<ShipPart>();
    }

    public static ShipConfigurationPacket FromBuffer(ReadOnlyMemory<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var packet = new ShipConfigurationPacket();

        // Read header (but don't store it)
        var header = reader.ReadHeader();

        // Read player ID
        packet.PlayerId = reader.ReadNtt();

        // Read part count
        var partCount = reader.ReadInt16();

        // Read center position
        packet.CenterX = reader.ReadSByte();
        packet.CenterY = reader.ReadSByte();

        // Read each part
        for (int i = 0; i < partCount; i++)
        {
            var part = new ShipPart(
                reader.ReadSByte(), // gridX (signed byte)
                reader.ReadSByte(), // gridY (signed byte)
                reader.ReadByte(), // type
                reader.ReadByte(), // shape
                reader.ReadByte()  // rotation
            );
            packet.Parts.Add(part);
        }

        return packet;
    }
}