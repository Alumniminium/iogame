using System;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class PlayerMovementPacket
{
    public NTT UniqueId { get; set; }
    public uint TickCounter { get; set; }
    public PlayerInput Inputs { get; set; }
    public Vector2 MousePosition { get; set; }

    public static Memory<byte> Create(NTT uniqueId, uint tickCounter, PlayerInput inputs, Vector2 mousePosition)
    {
        using var writer = new PacketWriter(PacketId.InputPacket);
        writer.WriteNtt(uniqueId)
              .WriteUInt32(tickCounter)
              .WriteUInt16((ushort)inputs)
              .WriteVector2(mousePosition);
        return writer.Finalize();
    }

    public static PlayerMovementPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);
        var header = reader.ReadHeader(); // Skip header

        return new PlayerMovementPacket
        {
            UniqueId = reader.ReadNtt(),
            TickCounter = reader.ReadUInt32(),
            Inputs = (PlayerInput)reader.ReadUInt16(),
            MousePosition = reader.ReadVector2()
        };
    }
}