using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using iogame.Simulation;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack = 1)]
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

        public static LoginResponsePacket Create(Player player)
        {
            return new LoginResponsePacket
            {
                Header = new Header(sizeof(LoginResponsePacket), 2),
                UniqueId = player.EntityId,
                TickCounter = Game.CurrentTick,
                Position = player.PositionComponent.Position,
                MapWidth = Game.MAP_WIDTH,
                MapHeight = Game.MAP_HEIGHT,
                ViewDistance = (ushort)player.Entity.Get<ViewportComponent>().ViewDistance,
                PlayerSize = player.ShapeComponent.Size,
                PlayerDrag = player.PhysicsComponent.Drag,
                PlayerElasticity = player.PhysicsComponent.Elasticity,
                PlayerMaxSpeed = player.SpeedComponent.Speed
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