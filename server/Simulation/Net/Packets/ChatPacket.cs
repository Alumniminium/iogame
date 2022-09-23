using System;
using System.Runtime.InteropServices;
using System.Text;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe ref struct ChatPacket
    {
        public Header Header;
        public uint UserId;
        public byte Channel;
        public byte MessageLength;
        public fixed byte Message[255];

        public string GetText()
        {
            fixed (byte* ptr = Message)
                return Encoding.ASCII.GetString(ptr, MessageLength);
        }

        public static implicit operator Memory<byte>(ChatPacket msg)
        {
            var buffer = new byte[sizeof(ChatPacket)];
            fixed (byte* p = buffer)
                *(ChatPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator ChatPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(ChatPacket*)p;
        }

        public static Memory<byte> Create(uint id, string text, byte channel = 0)
        {
            var packet = new ChatPacket
            {
                Header = new Header(sizeof(ChatPacket) - 255 + text.Length, PacketId.ChatPacket),
                UserId = id,
                Channel = channel,
                MessageLength = (byte)text.Length
            };
            for (int i = 0; i < text.Length; i++)
                packet.Message[i] = (byte)text[i];
            return packet;
        }
    }
}