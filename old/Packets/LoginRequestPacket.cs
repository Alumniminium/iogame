using System;
using System.Text;

namespace server.Simulation.Net.Packets
{
    internal unsafe ref struct LoginRequestPacket
    {
        public Header Header;
        public fixed byte Username[17];
        public fixed byte Password[17];

        public string GetUsername()
        {
            var len = Username[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = Username[1 + i];
            return Encoding.ASCII.GetString(txtBytes);
        }
        public string GetPassword()
        {
            var len = Password[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = Password[1 + i];
            return Encoding.ASCII.GetString(txtBytes);
        }

        public static LoginRequestPacket Create(string user)
        {
            var packet = new LoginRequestPacket();
            packet.Header = new Header(sizeof(LoginRequestPacket), PacketId.LoginRequest);
            var userBytes = Encoding.ASCII.GetBytes(user);
            packet.Username[0] = (byte)userBytes.Length;
            for (var i = 0; i < userBytes.Length; i++)
                packet.Username[1 + i] = userBytes[i];
            return packet;
        }

        public static implicit operator Memory<byte>(LoginRequestPacket msg)
        {
            var buffer = new byte[sizeof(LoginRequestPacket)];
            fixed (byte* p = buffer)
                *(LoginRequestPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator LoginRequestPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(LoginRequestPacket*)p;
        }
    }
}