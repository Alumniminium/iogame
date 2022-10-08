using System.Runtime.InteropServices;
using System.Text;
using Packets.Enums;

namespace Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe ref struct ChatPacket
    {
        public Header Header;
        public uint UserId;
        public byte Channel;
        public fixed byte Message[256];

        public string GetText()
        {
            fixed (byte* ptr = Message)
                return Encoding.ASCII.GetString(ptr, Message[0]);
        }

        public static implicit operator Memory<byte>(ChatPacket msg)
        {
            var buffer = new byte[sizeof(ChatPacket) - 255 + msg.Message[0]];
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
            };
            packet.Message[0] = (byte)text.Length;
            for (int i =1; i < text.Length; i++)
                packet.Message[i] = (byte)text[i];
            return packet;
        }
    }
}