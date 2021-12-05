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
        public Vector2 Velocity;
        public uint MaxSpeed;


        public static SpawnPacket Create(ref PixelEntity entity)
        {
            ref readonly var pos = ref entity.Get<PositionComponent>();
            ref readonly var shp = ref entity.Get<ShapeComponent>();
            ref readonly var hlt = ref entity.Get<HealthComponent>();
            ref readonly var phy = ref entity.Get<PhysicsComponent>();
            ref readonly var vel = ref entity.Get<VelocityComponent>();
            ref readonly var spd = ref entity.Get<SpeedComponent>();

            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), 1015),
                UniqueId = entity.EntityId,
                OwnerId = entity.Parent,
                Direction = pos.Rotation,
                Size = shp.Size,
                MaxHealth = hlt.MaxHealth,
                CurHealth = (int)hlt.Health,
                Color = shp.Color,
                BorderColor = shp.BorderColor,
                Drag = phy.Drag,
                Sides = shp.Sides,
                Position = pos.Position,
                Velocity = vel.Velocity,
                MaxSpeed = spd.Speed
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