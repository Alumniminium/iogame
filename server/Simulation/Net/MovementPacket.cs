using System;
using System.Numerics;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class MovementPacket
{
    public NTT UniqueId { get; set; }
    public uint TickCounter { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Rotation { get; set; }

    public static Memory<byte> Create(NTT uniqueId, long tickCounter, Vector2 position, Vector2 velocity, float rotation)
    {
        using var writer = new PacketWriter(PacketId.MovePacket);
        writer.WriteNtt(uniqueId)
              .WriteUInt32((uint)tickCounter)
              .WriteVector2(position)
              .WriteVector2(velocity)
              .WriteFloat(rotation);
        return writer.Finalize();
    }

    public static MovementPacket Read(Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer);

        return new MovementPacket
        {
            UniqueId = reader.ReadNtt(),
            TickCounter = reader.ReadUInt32(),
            Position = reader.ReadVector2(),
            Velocity = reader.ReadVector2(),
            Rotation = reader.ReadFloat()
        };
    }
}