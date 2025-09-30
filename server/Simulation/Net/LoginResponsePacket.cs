using System;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class LoginResponsePacket
{
    public NTT UniqueId { get; set; }
    public uint TickCounter { get; set; }
    public int MapWidth { get; set; }
    public int MapHeight { get; set; }
    public ushort ViewDistance { get; set; }

    public static Memory<byte> Create(NTT uniqueId, long tickCounter, Vector2 position, int mapWidth, int mapHeight, ushort viewDistance, uint playerColor)
    {
        using var writer = new PacketWriter(PacketId.LoginResponse);
        writer.WriteNtt(uniqueId)
              .WriteUInt32((uint)tickCounter)
              .WriteVector2(position)
              .WriteInt32(mapWidth)
              .WriteInt32(mapHeight)
              .WriteUInt16(viewDistance)
              .WriteUInt32(playerColor);
        return writer.Finalize();
    }

    public static LoginResponsePacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new LoginResponsePacket
        {
            UniqueId = reader.ReadNtt(),
            TickCounter = reader.ReadUInt32(),
            MapWidth = reader.ReadInt32(),
            MapHeight = reader.ReadInt32(),
            ViewDistance = reader.ReadUInt16(),
        };
    }
}