using System.Numerics;
using System.Runtime.InteropServices;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public unsafe struct ResourceSpawnPacket
    {
        public Header Header;
        public uint UniqueId;
        public ushort ResourceId;
        public float Direction;
        public Vector2 Position; 
        public Vector2 Velocity; 

        public static ResourceSpawnPacket Create(Entity entity)
        {
            return new ResourceSpawnPacket
            {
                Header = new Header(sizeof(ResourceSpawnPacket), 1116),
                UniqueId = entity.UniqueId,
                ResourceId = (ushort)(entity is YellowSquare ? 0 : entity is RedTriangle ? 1 : entity is PurplePentagon ? 2 : 3),
                Direction = entity.PositionComponent.Rotation,
                Position = entity.PositionComponent.Position,
                Velocity = entity.VelocityComponent.Movement,
            };
        }

        public static implicit operator byte[](ResourceSpawnPacket msg)
        {
            var buffer = new byte[sizeof(ResourceSpawnPacket)];
            fixed (byte* p = buffer)
                *(ResourceSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator ResourceSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(ResourceSpawnPacket*)p;
            }
        }
    }
}