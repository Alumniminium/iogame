
using System;
using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Net
{
    public static class PacketHandler
    {
        public static async Task Handle(Player player, byte[] buffer)
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
                        //TODO: Authenticate

                        await player.Send(LoginResponsePacket.Create(player.UniqueId, player.Position));
                        // Console.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.UniqueId)
                            return; // hax

                        var ticks = packet.TickCounter;

                        player.AddMovement(ticks, packet.Up,packet.Down,packet.Left,packet.Right);


                        player.Up = packet.Up;
                        player.Down = packet.Down;
                        player.Left = packet.Left;
                        player.Right = packet.Right;

                        // Console.WriteLine($"Movement Packet from Player {player.UniqueId}: Up:{player.Up} Down:{player.Down} Left:{player.Left} Right:{player.Right}");
                        break;
                    }
                    case 9000:
                    {
                        var packet = (PingPacket)buffer;
                        var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                        packet.Ping = (ushort)(delta / 10000);

                        await player.Send(packet);
                        break;
                    }
            }
        }
    }
}