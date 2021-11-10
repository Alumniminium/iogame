using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Components;
using iogame.Simulation.Entities;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Net
{
    public static unsafe class PacketHandler
    {
        public static void Process(Player player, byte[] buffer)
        {
            if (buffer == null) // shit why was this null
                return;

            var id = BitConverter.ToUInt16(buffer, 2);
            FConsole.WriteLine("Processing " + id);

            switch (id)
            {
                case 1:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        player.Name = packet.GetUsername();
                        player.Password = packet.GetPassword();
                        var point = SpawnManager.GetPlayerSpawnPoint();
                        // auth
                        var pos = new PositionComponent(point);
                        var vel = new VelocityComponent(0, 0);
                        var spd = new SpeedComponent(1500);
                        var shp = new ShapeComponent(sides: 32, size: 200);
                        var hlt = new HealthComponent(1000, 1000, 0);
                        var phy = new PhysicsComponent((float)Math.Pow(shp.Size, 3), 1, 0.999f);
                        player.Entity = World.CreateEntity(IdGenerator.Get<Player>());
                        player.Entity.AttachTo(player);
                        World.Players.Add(player.EntityId, player);
                        player.Entity.Add(vel);
                        player.Entity.Add(shp);
                        player.Entity.Add(hlt);
                        player.Entity.Add(phy);
                        player.Entity.Add(pos);
                        player.Entity.Add(spd);
                        player.Entity.Add<InputComponent>();

                        player.Send(LoginResponsePacket.Create(player));
                        player.Send(ChatPacket.Create("Server", $"{packet.GetUsername()} joined!"));
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1004:
                    {
                        var packet = (ChatPacket)buffer;
                        var user = packet.GetUsername();
                        var message = packet.GetText();


                        foreach (var kvp in World.Players)
                        {
                            if (kvp.Value.Name == user)
                                continue;

                            kvp.Value.Send(packet);
                        }
                        FConsole.WriteLine($"ChatPacket from {player.Name} - {user}: {message}");
                        break;
                    }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.EntityId)
                            return; // hax

                        var ticks = packet.TickCounter;

                        // player.AddMovement(ticks, packet.Up,packet.Down,packet.Left,packet.Right);

                        player.InputComponent.Up = packet.Up;
                        player.InputComponent.Down = packet.Down;
                        player.InputComponent.Left = packet.Left;
                        player.InputComponent.Right = packet.Right;
                        player.InputComponent.Fire = packet.Fire;
                        player.InputComponent.X = packet.X;
                        player.InputComponent.Y =  packet.Y;
                        player.FireDir = (float)Math.Atan2(packet.Y - player.PositionComponent.Position.Y, packet.X - player.PositionComponent.Position.X);
                        
                        FConsole.WriteLine($"Movement Packet from Player {player.EntityId}: {(packet.Up ? "Up" : "")} {(packet.Down ? "Down" : "")} {(packet.Left ? "Left" : "")} {(packet.Right ? "Right" : "")} X: ${packet.X},Y: ${packet.Y}");
                        break;
                    }
                case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if (player.EntityId != packet.UniqueId)
                            return; //hax

                        if (World.ShapeEntities.TryGetValue(packet.EntityId, out var entity))
                        {

                            // if (entity is not Player)
                            //     player.Send(ResourceSpawnPacket.Create(entity));
                            // else
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