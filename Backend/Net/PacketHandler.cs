
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Entities;

namespace iogame.Net
{
    public static unsafe class PacketHandler
    {
        public static void Handle(Player player, byte[] buffer)
        {
            var length = BitConverter.ToUInt16(buffer, 0);
            var id = BitConverter.ToUInt16(buffer, 2);

            switch (id)
            {
                case 1:
                {
                    var packet = (LoginRequestPacket)buffer;
                    player.Name = packet.GetUsername();
                    player.Password = packet.GetPassword();

                    // Authenticate

                    player.Send(LoginResponsePacket.Create(1000000,new Vector2(100,100)));
                    Console.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                    break;
                }
            }
        }
    }
}