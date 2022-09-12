using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct AsteroidSpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public Vector2 Position;
        public byte PointCount;
        public fixed float Points[32];

        public static AsteroidSpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var shp = ref ntt.Get<PolygonComponent>();
            ref readonly var pos = ref ntt.Get<PhysicsComponent>();

            var packet = new AsteroidSpawnPacket
            {
                Header = new Header(sizeof(AsteroidSpawnPacket), 1117),
                UniqueId = ntt.Id,
                Position = pos.Position,
                PointCount = (byte)shp.Points.Count
            };

            for (var i = 0; i < shp.Points.Count; i++)
            {
                var point = shp.Points[i];

                packet.Points[i * 2] = point.X;
                packet.Points[i * 2 + 1] = point.Y;
            }

            return packet;
        }

        public static implicit operator byte[](AsteroidSpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(AsteroidSpawnPacket));
            fixed (byte* p = buffer)
                *(AsteroidSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator AsteroidSpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(AsteroidSpawnPacket*)p;
        }
    }
}