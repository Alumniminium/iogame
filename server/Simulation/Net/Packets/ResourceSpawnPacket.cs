using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct ResourceSpawnPacket
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
                Direction = phy.RotationRadians,
                Position = phy.Position,
                // Velocity = vel.Velocity,
            };
        }

        public static implicit operator Memory<byte>(ResourceSpawnPacket msg)
        {
            var buffer = new byte[sizeof(ResourceSpawnPacket)];
            fixed (byte* p = buffer)
                *(ResourceSpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator ResourceSpawnPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
            {
                return *(ResourceSpawnPacket*)p;
            }
        }
    }
}