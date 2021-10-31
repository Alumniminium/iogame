using System.Numerics;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{

    public unsafe struct LoginResponsePacket
    {
        public Header Header;
        public uint UniqueId;
        public uint TickCounter;
        public Vector2 Position;
        public int MapWidth;
        public int MapHeight;
        public ushort ViewDistance;

        public static LoginResponsePacket Create(uint uniqueId, Vector2 position)
        {
            return new LoginResponsePacket
            {
                Header = new Header(sizeof(LoginResponsePacket), 2),
                UniqueId = uniqueId,
                TickCounter = Game.CurrentTick,
                Position = position,
                MapWidth = Game.MAP_WIDTH,
                MapHeight = Game.MAP_HEIGHT,
                ViewDistance = Player.VIEW_DISTANCE,
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