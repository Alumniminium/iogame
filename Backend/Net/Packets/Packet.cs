using System.Numerics;

namespace iogame.Net.Packets;

public unsafe struct LoginResponsePacket
{
    public Header Header;
    public uint UniqueId;
    public Vector2 Position;

    public static LoginResponsePacket Create(uint uniqueId, Vector2 position)
    {
        return new LoginResponsePacket
        {
            Header= new Header(16,2),
            UniqueId = uniqueId,
            Position = position
        }; 
    }

    public static implicit operator byte[](LoginResponsePacket msg)
    {
        var buffer = new byte[sizeof(LoginResponsePacket)];
        fixed (byte* p = buffer)
            *(LoginResponsePacket*)p = *&msg;
        return buffer;
    }
}