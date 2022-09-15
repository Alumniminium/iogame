using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ResourceSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort ResourceId;
        public float Direction;
        public Vector2 Position;
        // public Vector2 Velocity; 

        public static ResourceSpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new ResourceSpawnPacket
            {
                Header = new Header(sizeof(ResourceSpawnPacket), 1116),
                UniqueId = ntt.Id,
                ResourceId = 4,
                Direction = phy.Rotation,
                Position = phy.Position,
                // Velocity = vel.Velocity,
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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct StructureSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort Width;
        public ushort Height;
        public float Direction;
        public Vector2 Position;

        public static StructureSpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new StructureSpawnPacket
            {
                Header = new Header(sizeof(StructureSpawnPacket), 1117),
                UniqueId = ntt.Id,
                Width = (ushort)phy.Width,
                Height = (ushort)phy.Height,
                Position = phy.Position,
                Direction = phy.Rotation,
                // Velocity = vel.Velocity,
            };
        }

        public static implicit operator byte[](StructureSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(StructureSpawnPacket));
            fixed (byte* p = buffer)
                *(StructureSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator StructureSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(StructureSpawnPacket*)p;
            }
        }
    }
}