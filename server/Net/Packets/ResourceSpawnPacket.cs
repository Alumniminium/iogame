using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using iogame.Simulation.Components;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public unsafe struct ResourceSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort ResourceId;
        public float Direction;
        public Vector2 Position; 
        public Vector2 Velocity; 

        public static ResourceSpawnPacket Create(ShapeEntity entity)
        {
            return new ResourceSpawnPacket
            {
                Header = new Header(sizeof(ResourceSpawnPacket), 1116),
                UniqueId = entity.EntityId,
                ResourceId = entity.ShapeComponent.Sides,
                Direction = entity.PositionComponent.Rotation,
                Position = entity.PositionComponent.Position,
                Velocity = entity.Entity.Has<VelocityComponent>() ? entity.VelocityComponent.Velocity : Vector2.Zero,
            };
        }

        public static implicit operator byte[](ResourceSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ResourceSpawnPacket));
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