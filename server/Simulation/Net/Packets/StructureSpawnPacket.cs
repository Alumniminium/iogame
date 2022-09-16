using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct BoxSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort Width;
        public ushort Height;
        public float Direction;
        public Vector2 Position;
        public uint Color;

        public static BoxSpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new BoxSpawnPacket
            {
                Header = new Header(sizeof(BoxSpawnPacket), 1117),
                UniqueId = ntt.Id,
                Width = (ushort)phy.Width,
                Height = (ushort)phy.Height,
                Position = phy.Position,
                Direction = phy.Rotation,
                Color = phy.Color
            };
        }

        public static implicit operator byte[](BoxSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(BoxSpawnPacket));
            fixed (byte* p = buffer)
                *(BoxSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator BoxSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(BoxSpawnPacket*)p;
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SphereSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ushort Radius;
        public float Direction;
        public Vector2 Position;
        public uint Color;

        public static SphereSpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new SphereSpawnPacket
            {
                Header = new Header(sizeof(SphereSpawnPacket), 1116),
                UniqueId = ntt.Id,
                Radius = (ushort)phy.Radius,
                Position = phy.Position,
                Direction = phy.Rotation,
                Color = phy.Color
            };
        }

        public static implicit operator byte[](SphereSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(SphereSpawnPacket));
            fixed (byte* p = buffer)
                *(SphereSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator SphereSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(SphereSpawnPacket*)p;
        }
    }
}