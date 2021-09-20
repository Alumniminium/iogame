using System.Numerics;
using iogame.Simulation;

namespace iogame.Net.Packets
{

    public unsafe struct LoginResponsePacket
    {
        public Header Header;
        public uint UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public ushort MapWidth;
        public ushort MapHeight;
        public ushort ViewportSize;
        public float Restitution;

        public static LoginResponsePacket Create(uint uniqueId, Vector2 position)
        {
            return new LoginResponsePacket
            {
                Header = new Header(16, 2),
                UniqueId = uniqueId,
                TickCounter = Game.TickCounter,
                Position = position,
                MapWidth = Game.MAP_WIDTH,
                MapHeight = Game.MAP_HEIGHT,
                ViewportSize = 600,
                Restitution = Game.DRAG
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
}