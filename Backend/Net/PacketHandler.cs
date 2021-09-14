
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation;
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
                        player.Position = new Vector2(Game.MAP_WIDTH / 2, Game.MAP_HEIGHT / 2);
                        // Authenticate

                        player.Send(LoginResponsePacket.Create(player.UniqueId, player.Position));
                        Console.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.UniqueId)
                            return; // hax

                        player.Up = packet.Up;
                        player.Down = packet.Down;
                        player.Left = packet.Left;
                        player.Right = packet.Right;

                        Console.WriteLine($"Movement Packet from Player {player.UniqueId}: Up:{player.Up} Down:{player.Down} Left:{player.Left} Right:{player.Right}");
                        break;
                    }
            }
        }
    }
}