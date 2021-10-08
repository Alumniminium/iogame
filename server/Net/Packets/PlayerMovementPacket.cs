using System.Runtime.InteropServices;

namespace iogame.Net.Packets
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PlayerMovementPacket
    {
        public Header Header;
        public uint UniqueId;
        public uint TickCounter;
        public byte Inputs;
        public float X;
        public float Y;

        public bool Up => GetBit(Inputs,0);
        public bool Down => GetBit(Inputs,1);
        public bool Left => GetBit(Inputs,2);
        public bool Right => GetBit(Inputs,3);
        public bool Fire => GetBit(Inputs,4);
        public bool AltFire => GetBit(Inputs, 5);
        public bool Boost => GetBit(Inputs,6);

        public static implicit operator PlayerMovementPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(PlayerMovementPacket*)p;
            }
        }
        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }
    }

}