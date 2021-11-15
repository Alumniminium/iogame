using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack =1)]
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


        public static SpawnPacket Create(ShapeEntity entity)
        {
            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), 1015),
                UniqueId = entity.EntityId,
                OwnerId = entity is Bullet b ? b.Owner.EntityId : 0,
                Direction = entity.PositionComponent.Rotation,
                Size = entity.ShapeComponent.Size,
                MaxHealth = entity.HealthComponent.MaxHealth,
                CurHealth = (int)entity.HealthComponent.Health,
                //Color = entity.FillColor,
                //BorderColor = entity.BorderColor,
                Drag = entity.PhysicsComponent.Drag,
                Sides = entity.ShapeComponent.Sides,
                Position = entity.PositionComponent.Position,
                Velocity = entity.VelocityComponent.Force,
                MaxSpeed = entity.SpeedComponent.Speed
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