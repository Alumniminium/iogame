using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct BoxSpawnPacket
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
                Direction = phy.RotationRadians,
                Color = phy.Color
            };
        }

        public static implicit operator Memory<byte>(BoxSpawnPacket msg)
        {
            var buffer = new byte[sizeof(BoxSpawnPacket)];
            fixed (byte* p = buffer)
                *(BoxSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator BoxSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(BoxSpawnPacket*)p;
            }
        }
    }
}