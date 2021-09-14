using System.Runtime.InteropServices;

namespace iogame.Net.Packets;

[StructLayout(LayoutKind.Sequential, Pack=1)]
public unsafe struct PlayerMovementPacket
{
    public Header Header;
    public uint UniqueId;
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;

    public static implicit operator PlayerMovementPacket(byte[] buffer)
    {
        fixed (byte* p = buffer)
        {
            return *(PlayerMovementPacket*)p;
        }
    }
}