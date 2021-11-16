using System.Numerics;
using iogame.ECS;
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

                        player.Entity = World.CreateEntity(IdGenerator.Get<Player>());

                        ref var pos = ref ComponentList<PositionComponent>.AddFor(player.Entity.EntityId);
                        ref var vel = ref ComponentList<VelocityComponent>.AddFor(player.Entity.EntityId);
                        ref var spd = ref ComponentList<SpeedComponent>.AddFor(player.Entity.EntityId);
                        ref var shp = ref ComponentList<ShapeComponent>.AddFor(player.Entity.EntityId);
                        ref var hlt = ref ComponentList<HealthComponent>.AddFor(player.Entity.EntityId);
                        ref var phy = ref ComponentList<PhysicsComponent>.AddFor(player.Entity.EntityId);
                        
                        pos.Position = point;
                        vel.Force = Vector2.Zero;
                        spd.Speed = 1500;
                        shp.Sides = 32;
                        shp.Size = 200;
                        hlt.Health = 1000;
                        hlt.MaxHealth = 1000;
                        hlt.HealthRegenFactor = 1;
                        phy.Mass = (float)Math.Pow(shp.Size, 3);
                        phy.Drag = 0.999f;
                        phy.Elasticity = 1;
                        
                        player.Entity.AttachTo(player);
                        World.Players.Add(player.EntityId, player);
                        player.Entity.Add(ref vel);
                        player.Entity.Add(ref shp);
                        player.Entity.Add(ref hlt);
                        player.Entity.Add(ref phy);
                        player.Entity.Add(ref pos);
                        player.Entity.Add(ref spd);
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