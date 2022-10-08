using System;
using System.Text;
using Packets.Enums;

namespace Packets
{
    public unsafe ref struct AssociateIdPacket
    {
        public Header Header;
        public int Id;
        public fixed byte Name[17];
        public string GetName()
        {
            var len = Name[0];
            var txtBytes = new byte[len];
            for (var i = 0; i < txtBytes.Length; i++)
                txtBytes[i] = Name[1 + i];
            return Encoding.ASCII.GetString(txtBytes);
        }

        public static AssociateIdPacket Create(int id, string name)
        {
            var packet = new AssociateIdPacket()
            {
                Header = new Header(sizeof(AssociateIdPacket) - 16 + name.Length, PacketId.AssociateId),
                Id = id
            };
            packet.Name[0] = (byte)name.Length;
            for (int i = 0; i < name.Length; i++)
                packet.Name[1 + i] = (byte)name[i];

            return packet;
        }

        public static implicit operator Memory<byte>(AssociateIdPacket msg)
        {
            Memory<byte> buffer = new byte[sizeof(AssociateIdPacket) - 16 + msg.Name[0]];
            fixed (byte* p = buffer.Span)
                *(AssociateIdPacket*)p = *&msg;
            return buffer;
        }
        public static implicit operator AssociateIdPacket(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(AssociateIdPacket*)p;
        }
    }
}