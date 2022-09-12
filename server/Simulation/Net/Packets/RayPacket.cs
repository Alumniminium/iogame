using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RayPacket
    {
        public Header Header;
        public int UniqueId;
        public int TargetUniqueId;
        public Vector2 Origin;
        public Vector2 Hit;

        public static RayPacket Create(in PixelEntity ntt, in PixelEntity hit, ref Vector2 hitPos)
        {
            ref readonly var aPhy = ref ntt.Get<PhysicsComponent>();
            ref readonly var bPhy = ref hit.Get<PhysicsComponent>();

            var packet = new RayPacket
            {
                Header = new Header(sizeof(RayPacket), 1118),
                UniqueId = Random.Shared.Next(int.MaxValue / 2, int.MaxValue),
                TargetUniqueId = hit.Id,
                Origin = aPhy.Position,
                Hit = hitPos,
            };

            return packet;
        }

        public static implicit operator byte[](RayPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(RayPacket));
            fixed (byte* p = buffer)
                *(RayPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator RayPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
                return *(RayPacket*)p;
        }
    }
}