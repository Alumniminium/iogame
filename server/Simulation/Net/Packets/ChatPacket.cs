using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace server.Simulation.Net.Packets
{
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    unsafe struct ChatPacket
    {
        public Header Header;
        public fixed byte Username[17];
        public fixed byte Text[257];

        public string GetUsername()
        {
            var len = Username[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = Username[1 + i];
            return Encoding.UTF8.GetString(txtBytes);
        }
        public string GetText()
        {
            var len = Text[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = Text[1 + i];
            return Encoding.UTF8.GetString(txtBytes);
        }

        public static implicit operator byte[](ChatPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ChatPacket));
            fixed (byte* p = buffer)
                *(ChatPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator ChatPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(ChatPacket*)p;
            }
        }

        public static byte[] Create(string from, string text)
        {
            var packet = new ChatPacket 
            {
                Header = new Header(sizeof(Header) + 18 + text.Length, 1004)
            };

            packet.Username[0] = (byte)from.Length;
            for (var i = 0; i < from.Length; i++)
                packet.Username[1 + i] = (byte)from[i];
            
            packet.Text[0] = (byte)text.Length;
            for (var i = 0; i < text.Length; i++)
                packet.Text[1 + i] = (byte)text[i];

            return packet;
        }
    }
}