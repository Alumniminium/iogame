using System.Buffers;
using System.Text;

namespace server.Simulation.Net.Packets
{

    unsafe struct LoginRequestPacket
    {
        public Header Header;
        private fixed byte _username[17];
        private fixed byte _password[17];

        public string GetUsername()
        {
            var len = _username[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = _username[1 + i];
            return Encoding.ASCII.GetString(txtBytes);
        }
        public string GetPassword()
        {
            var len = _password[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = _password[1 + i];
            return Encoding.ASCII.GetString(txtBytes);
        }

        public static implicit operator byte[](LoginRequestPacket msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(LoginRequestPacket));
            fixed (byte* p = buffer)
                *(LoginRequestPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator LoginRequestPacket(byte[] buffer)
        {
            fixed (byte* p = buffer)
            {
                return *(LoginRequestPacket*)p;
            }
        }
    }
}