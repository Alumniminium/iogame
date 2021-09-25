using System.Numerics;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Net.Packets
{
    public unsafe struct SpawnPacket
    {
        public Header Header;
        public uint UniqueId;
        public ushort Direction;
        public ushort Size;
        public ushort Mass;
        public int MaxHealth;
        public int CurHealth;
        public uint Color;
        public uint BorderColor;
        public float Drag;
        public Vector2 Position; 
        public Vector2 Velocity; 


        public static SpawnPacket Create(Entity entity)
        {
            return new SpawnPacket
            {
                Header = new Header(sizeof(SpawnPacket), 1015),
                UniqueId = entity.UniqueId,
                Direction = (ushort)entity.Direction,
                Size = entity.Size,
                Mass = entity.Mass,
                MaxHealth = entity.MaxHealth,
                CurHealth = (int)entity.Health,
                Color = 0,
                BorderColor = 0,
                Position = entity.Position,
                Velocity = entity.Velocity
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