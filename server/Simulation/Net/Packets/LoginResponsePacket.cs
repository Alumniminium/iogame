using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using iogame.ECS;
using iogame.Simulation;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LoginResponsePacket
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
        public uint PlayerMaxSpeed;

        public static LoginResponsePacket Create(PixelEntity player)
        {
            ref readonly var pos = ref player.Get<PositionComponent>();
            ref readonly var shp = ref player.Get<ShapeComponent>();
            ref readonly var phy = ref player.Get<PhysicsComponent>();
            ref readonly var spd = ref player.Get<SpeedComponent>();
            ref readonly var vwp = ref player.Get<ViewportComponent>();

            return new LoginResponsePacket
            {
                Header = new Header(sizeof(LoginResponsePacket), 2),
                UniqueId = player.EntityId,
                TickCounter = Game.CurrentTick,
                MapWidth = Game.MAP_WIDTH,
                MapHeight = Game.MAP_HEIGHT,
                Position = pos.Position,
                PlayerSize = shp.Size,
                PlayerDrag = phy.Drag,
                PlayerElasticity = phy.Elasticity,
                PlayerMaxSpeed = spd.Speed,
                ViewDistance = (ushort)vwp.ViewDistance,
            };
        }

        public static implicit operator byte[](LoginResponsePacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(LoginResponsePacket));
            fixed (byte* p = buffer)
                *(LoginResponsePacket*)p = *&msg;
            return buffer;
        }
    }
}