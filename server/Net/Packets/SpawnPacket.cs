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
        public uint UniqueId;
        public float Direction;
        public ushort Size;
        public uint Mass;
        public int MaxHealth;
        public int CurHealth;
        public uint Color;
        public uint BorderColor;
        public float Drag;       
        public byte Sides;
        public Vector2 Position; 
        public Vector2 Velocity; 
        public uint MaxSpeed;


        public static SpawnPacket Create(Entity entity)
        {
            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), 1015),
                UniqueId = entity.UniqueId,
                Direction = entity.Direction,
                Size = entity.Size,
                Mass = (uint)entity.Mass,
                MaxHealth = entity.MaxHealth,
                CurHealth = (int)entity.Health,
                Color = entity.FillColor,
                BorderColor = entity.BorderColor,
                Drag = Game.DRAG,
                Sides = entity.Sides,
                Position = entity.Position,
                Velocity = entity.Velocity,
                MaxSpeed = entity.MaxSpeed
            };
        }

        public static implicit operator byte[](SpawnPacket msg)
        {
            var buffer = new byte[sizeof(SpawnPacket)];
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