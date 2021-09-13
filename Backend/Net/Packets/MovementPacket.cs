using System.Numerics;

namespace iogame.Net.Packets;

public unsafe struct MovementPacket
{
    public Header Header;
    public uint UniqueId;
    public Vector2 Position;
    public Vector2 Velocity;

    public static MovementPacket Create(uint uniqueId, Vector2 position, Vector2 velocity)
    {
        return new MovementPacket
        {
            Header = new Header((ushort)sizeof(MovementPacket), 1005),
            UniqueId = uniqueId,
            Position = position,
            Velocity = velocity
        };
    }

    public static implicit operator byte[](MovementPacket msg)
    {
        var buffer = new byte[sizeof(MovementPacket)];
        fixed (byte* p = buffer)
            *(MovementPacket*)p = *&msg;
        return buffer;
    }
}