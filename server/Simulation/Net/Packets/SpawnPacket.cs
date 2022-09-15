using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SpawnPacket
    {
        public Header Header;
        public int UniqueId;
        public int OwnerId;
        public float Direction;
        public ushort Size;
        public int MaxHealth;
        public int CurHealth;
        public uint Color;
        public uint BorderColor;
        public float Drag;
        public byte Sides;
        public Vector2 Position;
        public uint MaxSpeed;


        public static SpawnPacket Create(in PixelEntity ntt)
        {
            ref readonly var hlt = ref ntt.Get<HealthComponent>();
            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), 1015),
                UniqueId = ntt.Id,
                OwnerId = ntt.Has<BulletComponent>() ? ntt.Get<BulletComponent>().Owner.Id : 0,
                Direction = phy.Rotation,
                Size = phy.Size,
                MaxHealth = hlt.MaxHealth,
                CurHealth = (int)hlt.Health,
                Color = (uint)phy.Color,
                BorderColor = (uint)phy.Color,
                Drag = phy.Drag,
                Sides = 4,
                Position = phy.Position,
            };
        }

        public static implicit operator byte[](SpawnPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(SpawnPacket));
            fixed (byte* p = buffer)
                *(SpawnPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator SpawnPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(SpawnPacket*)p;
            }
        }
    }
}