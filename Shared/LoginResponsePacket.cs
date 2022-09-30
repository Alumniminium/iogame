using System.Numerics;
using System.Runtime.InteropServices;
using Packets.Enums;

namespace Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct LoginResponsePacket
    {
        public Header Header;
        public int UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public int MapWidth;
        public int MapHeight;
        public ushort ViewDistance;
        public float PlayerSize;
        public float PlayerDrag;
        public float PlayerElasticity;
        public ushort PlayerMaxSpeed;
        public uint PlayerColor;

        public static LoginResponsePacket Create(int uniqueId, uint tickCounter, Vector2 position, int mapWidth, int mapHeight, ushort viewDistance, float playerSize, float playerDrag, float playerElasticity, ushort playerMaxSpeed, uint playerColor)
        {
            return new LoginResponsePacket
            {
                Header = new Header(sizeof(LoginResponsePacket), PacketId.LoginResponse),
                UniqueId = uniqueId,
                TickCounter = tickCounter,
                Position = position,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                ViewDistance = viewDistance,
                PlayerSize = playerSize,
                PlayerDrag = playerDrag,
                PlayerElasticity = playerElasticity,
                PlayerMaxSpeed = playerMaxSpeed,
                PlayerColor = playerColor
            };
        }

        public static implicit operator Memory<byte>(LoginResponsePacket msg)
        {
            var buffer = new byte[sizeof(LoginResponsePacket)];
            fixed (byte* p = buffer)
                *(LoginResponsePacket*)p = *&msg;
            return buffer;
        }

        public static implicit operator LoginResponsePacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(LoginResponsePacket*)p;
        }
    }
}