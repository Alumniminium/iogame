using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
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

        public static ResourceSpawnPacket Create(ref PixelEntity entity)
        {
            ref readonly var shp = ref entity.Get<ShapeComponent>();
            ref readonly var pos = ref entity.Get<PositionComponent>();
            ref readonly var vel = ref entity.Get<VelocityComponent>();

            return new ResourceSpawnPacket
            {
                Header = new Header(sizeof(ResourceSpawnPacket), 1116),
                UniqueId = entity.EntityId,
                ResourceId = shp.Sides,
                Direction = pos.Rotation,
                Position = pos.Position,
                Velocity = vel.Velocity,
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