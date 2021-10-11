using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame.Net
{
    public static class PacketHandler
    {
        public static async Task Handle(Player player, byte[] buffer)
        {
            var id = BitConverter.ToUInt16(buffer, 2);

            switch (id)
            {
                case 1:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        player.Name = packet.GetUsername();
                        player.Password = packet.GetPassword();

                        // auth

                        var pos = SpawnManager.GetPlayerSpawnPoint();
                        player.Position = pos;
                        Game.AddPlayer(player);
                        
                        await player.Send(LoginResponsePacket.Create(player.UniqueId, player.Position));
                        await player.Send(ChatPacket.Create("Server", $"{packet.GetUsername()} joined!"));
                        Console.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1004:
                {
                    var packet = (ChatPacket)buffer;
                    var user = packet.GetUsername();
                    var message = packet.GetText();

                    foreach(var kvp in Collections.Players)
                    {
                        if(kvp.Key == packet.UniqueId)
                            continue;
                        
                        await kvp.Value.Send(packet);
                    }

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
                        player.Fire = packet.Fire;
                        player.FireDir = (float)Math.Atan2(packet.Y - player.Position.Y, packet.X - player.Position.X);

                        Console.WriteLine($"Movement Packet from Player {player.UniqueId}: Up:{player.Up} Down:{player.Down} Left:{player.Left} Right:{player.Right} Fire: ${packet.Fire} X: ${packet.X},Y: ${packet.Y}");
                        break;
                    }
                    case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        Console.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if(player.UniqueId != packet.UniqueId)
                            return; //hax

                        if(Collections.Entities.TryGetValue(packet.EntityId, out var entity))
                        {
                                                       
                            if(entity is YellowSquare || entity is RedTriangle || entity is PurpleOctagon)
                                await player.Send(ResourceSpawnPacket.Create(entity));
                            else
                                await player.Send(SpawnPacket.Create(entity));
                                
                            Console.WriteLine($"Spawnpacket sent for {packet.EntityId}");
                        }
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