using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Entities;
using iogame.Util;

namespace iogame.Net
{
    public static unsafe class PacketHandler
    {
        public static void Process(Player player, byte[] buffer)
        {
            if(buffer == null) // shit why was this null
                return;

            var id = BitConverter.ToUInt16(buffer, 2);
            FConsole.WriteLine("Processing " +id);

            switch (id)
            {
                case 1:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        player.Name = packet.GetUsername();
                        player.Password = packet.GetPassword();
                        // auth
                        player.UniqueId = IdGenerator.Get<Player>();

                        var pos = SpawnManager.GetPlayerSpawnPoint();
                        player.PositionComponent.Position = pos;
                        Game.AddEntity(player);
                        
                        player.Send(LoginResponsePacket.Create(player.UniqueId, player.PositionComponent.Position));
                        player.Send(ChatPacket.Create("Server", $"{packet.GetUsername()} joined!"));
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
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
                        
                        kvp.Value.Send(packet);
                    }
                    FConsole.WriteLine($"ChatPacket from {player.Name} - {user}: {message}");   
                    break;
                }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.UniqueId)
                            return; // hax

                        var ticks = packet.TickCounter;

                        // player.AddMovement(ticks, packet.Up,packet.Down,packet.Left,packet.Right);


                        player.Up = packet.Up;
                        player.Down = packet.Down;
                        player.Left = packet.Left;
                        player.Right = packet.Right;
                        player.Fire = packet.Fire;
                        player.FireDir = (float)Math.Atan2(packet.Y - player.PositionComponent.Position.Y, packet.X - player.PositionComponent.Position.X);

                        // FConsole.WriteLine($"Movement Packet from Player {player.UniqueId}: Up:{player.Up} Down:{player.Down} Left:{player.Left} Right:{player.Right} Fire: ${packet.Fire} X: ${packet.X},Y: ${packet.Y}");
                        break;
                    }
                    case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if(player.UniqueId != packet.UniqueId)
                            return; //hax

                        if(Collections.Entities.TryGetValue(packet.EntityId, out var entity))
                        {
                                                       
                            if(entity is not Player)
                                player.Send(ResourceSpawnPacket.Create(entity));
                            else
                                player.Send(SpawnPacket.Create(entity));
                                
                            FConsole.WriteLine($"Spawnpacket sent for {packet.EntityId}");
                        }
                        break;
                    }
                    case 9000:
                    {
                        var packet = (PingPacket)buffer;
                        var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                        packet.Ping = (ushort)(delta / 10000);

                        player.Send(packet);
                        break;
                    }
            }
        }
    }
}