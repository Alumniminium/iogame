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
                        player.Entity.AttachTo(player);

                        ref var pos = ref player.Entity.Add<PositionComponent>();
                        ref readonly var vel = ref player.Entity.Add<VelocityComponent>();
                        ref var spd = ref player.Entity.Add<SpeedComponent>();
                        ref var shp = ref player.Entity.Add<ShapeComponent>();
                        ref var hlt = ref player.Entity.Add<HealthComponent>();
                        ref var phy = ref player.Entity.Add<PhysicsComponent>();
                        ref readonly var inp = ref player.Entity.Add<InputComponent>();
                        
                        pos.Position = point;
                        spd.Speed = 100;
                        shp.Sides = 32;
                        shp.Size = 10;
                        hlt.Health = 100;
                        hlt.MaxHealth = 100;
                        hlt.HealthRegenFactor = 10;
                        phy.Mass = (float)Math.Pow(shp.Size*2, 3);
                        phy.Drag = 0.01f;
                        phy.Elasticity = 0.75f;
                        World.Players.Add(player.EntityId, player);

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

                        if(packet.Up)
                            player.InputComponent.MovementAxis.Y = -1;
                        else if(packet.Down)
                            player.InputComponent.MovementAxis.Y = 1;
                        else
                            player.InputComponent.MovementAxis.Y = 0;

                        if(packet.Left)
                            player.InputComponent.MovementAxis.X = -1;
                        else if(packet.Right)
                            player.InputComponent.MovementAxis.X = 1;
                        else
                            player.InputComponent.MovementAxis.X = 0;

                        player.InputComponent.Fire = packet.Fire;
                        player.InputComponent.MousePositionWorld = new Vector2(packet.X, packet.Y);
                        
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

                            if (entity is not Player)
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