using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct LoginResponsePacket
    {
        private Header Header;
        private int UniqueId;
        private uint TickCounter;
        private Vector2 Position;
        private int MapWidth;
        private int MapHeight;
        private ushort ViewDistance;
        private float PlayerSize;
        private float PlayerDrag;
        private float PlayerElasticity;
        private ushort PlayerMaxSpeed;

        public static LoginResponsePacket Create(PixelEntity player)
        {
            ref readonly var phy = ref player.Get<PhysicsComponent>();
            ref readonly var eng = ref player.Get<EngineComponent>();
            ref readonly var vwp = ref player.Get<ViewportComponent>();

            return new LoginResponsePacket
            {
                Header = new Header(sizeof(LoginResponsePacket), 2),
                UniqueId = player.Id,
                TickCounter = Game.CurrentTick,
                MapWidth = (int)Game.MapSize.X,
                MapHeight = (int)Game.MapSize.Y,
                Position = phy.Position,
                PlayerSize = phy.Size,
                PlayerDrag = phy.Drag,
                PlayerElasticity = phy.Elasticity,
                PlayerMaxSpeed = eng.MaxPropulsion,
                ViewDistance = (ushort)(vwp.Viewport.Width),
            };
        }

        public static implicit operator Memory<byte>(LoginResponsePacket msg)
        {
            var buffer = new byte[sizeof(LoginResponsePacket)];
            fixed (byte* p = buffer)
                *(LoginResponsePacket*)p = *&msg;
            return buffer;
        }

        public static implicit operator LoginResponsePacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(LoginResponsePacket*)p;
        }
    }
}