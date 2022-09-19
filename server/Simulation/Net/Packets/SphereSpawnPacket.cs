using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct SphereSpawnPacket
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
                Direction = phy.RotationRadians,
                Color = phy.Color
            };
        }

        public static implicit operator Memory<byte>(SphereSpawnPacket msg)
        {
            var buffer = new byte[sizeof(SphereSpawnPacket)];
            fixed (byte* p = buffer)
                *(SphereSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator SphereSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(SphereSpawnPacket*)p;
        }
    }
}