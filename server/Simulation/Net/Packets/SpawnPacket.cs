using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct SpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public ShapeType ShapeType;
        public float Width;
        public float Height;
        public float Direction;
        public Vector2 Position;
        public uint Color;

        public static SpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), PacketId.CustomSpawnPacket),
                UniqueId = ntt.Id,
                ShapeType = phy.ShapeType == ShapeType.Circle ? ShapeType.Circle : phy.Sides == 3 ? ShapeType.Triangle : ShapeType.Box,
                Width = phy.ShapeType == ShapeType.Circle ? phy.Radius : phy.Width,
                Height = phy.ShapeType == ShapeType.Circle ? phy.Radius : phy.Height,
                Position = phy.Position,
                Direction = phy.RotationRadians,
                Color = phy.Color
            };
        }

        public static implicit operator Memory<byte>(SpawnPacket msg)
        {
            var buffer = new byte[sizeof(SpawnPacket)];
            fixed (byte* p = buffer)
                *(SpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator SpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(SpawnPacket*)p;
        }
    }
}