using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Entities;
using server.Simulation.Managers;
using server.Simulation.Net.Packets;

namespace server.Simulation.Net
{
    public static class PacketHandler
    {
        public static void Process(PixelEntity player, byte[] buffer)
        {
            var id = BitConverter.ToUInt16(buffer, 2);
            FConsole.WriteLine($"Processing {id}");
            
            switch (id)
            {
                case 1:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        // player.Name = packet.GetUsername();
                        // player.Password = packet.GetPassword();
                        var shpPlayer = new Player
                        {
                            Entity = player
                        };
                        player.AttachTo(shpPlayer);
                        // auth

                        ref var pos = ref player.Add<PositionComponent>();
                        ref readonly var vel = ref player.Add<VelocityComponent>();
                        ref var spd = ref player.Add<SpeedComponent>();
                        ref var shp = ref player.Add<ShapeComponent>();
                        ref var hlt = ref player.Add<HealthComponent>();
                        ref var phy = ref player.Add<PhysicsComponent>();
                        ref var vwp = ref player.Add<ViewportComponent>();
                        ref readonly var inp = ref player.Add<InputComponent>();
                        ref var col = ref player.Add<ColliderComponent>();
                        var shpEntity = PixelWorld.GetAttachedShapeEntity(ref player);

                        vwp.ViewDistance = 500;

                        vwp.EntitiesVisible = Array.Empty<ShapeEntity>();
                        vwp.EntitiesVisibleLastSync = Array.Empty<ShapeEntity>();
                        pos.Position = SpawnManager.GetPlayerSpawnPoint();
                        spd.Speed = 50;
                        shp.Sides = 32;
                        shp.Size = 20;
                        shp.Color = Convert.ToUInt32("00bbf9", 16);
                        hlt.Health = 100;
                        hlt.MaxHealth = 100;
                        hlt.HealthRegenFactor = 10;
                        phy.Mass = (float)Math.Pow(shp.Size, 3);
                        phy.Drag = 0.01f;
                        phy.Elasticity = 1f;
                        PixelWorld.Players.Add(player.EntityId, (Player)shpEntity);

                        shpEntity.Rect = new System.Drawing.RectangleF(pos.Position.X - shp.Radius, pos.Position.Y - shp.Radius, shp.Size, shp.Size);
                        col.EntityId = player.EntityId;
                        player.NetSync(LoginResponsePacket.Create(player));
            lock(Game.Tree)
                        Game.Tree.Add(shpEntity);
                        Game.Broadcast(ChatPacket.Create("Server", $"{packet.GetUsername()} joined!"));
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        break;
                    }
                case 1004:
                    {
                        var packet = (ChatPacket)buffer;
                        var user = packet.GetUsername();
                        var message = packet.GetText();

                        Game.Broadcast(packet);
                        FConsole.WriteLine($"ChatPacket from {user}: {message}");
                        break;
                    }
                case 1005:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.EntityId)
                            return; // hax

                        var ticks = packet.TickCounter;
                        ref var inp = ref player.Get<InputComponent>();

                        // player.AddMovement(ticks, packet.Up,packet.Down,packet.Left,packet.Right);

                        if (packet.Up)
                            inp.MovementAxis.Y = -1;
                        else if (packet.Down)
                            inp.MovementAxis.Y = 1;
                        else
                            inp.MovementAxis.Y = 0;

                        if (packet.Left)
                            inp.MovementAxis.X = -1;
                        else if (packet.Right)
                            inp.MovementAxis.X = 1;
                        else
                            inp.MovementAxis.X = 0;

                        inp.Fire = packet.Fire;
                        inp.MousePositionWorld = new Vector2(packet.X, packet.Y);

                        FConsole.WriteLine($"Movement Packet from Player {player.EntityId}: {(packet.Up ? "Up" : "")} {(packet.Down ? "Down" : "")} {(packet.Left ? "Left" : "")} {(packet.Right ? "Right" : "")} X: ${packet.X},Y: ${packet.Y}");
                        break;
                    }
                case 1016:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if (player.EntityId != packet.UniqueId)
                            return; //hax

                        if (!PixelWorld.EntityExists(packet.EntityId))
                            return;

                        ref var entity = ref PixelWorld.GetEntity(packet.EntityId);

                        if (entity.IsPlayer())
                        {
                            if (entity.Has<PositionComponent, ShapeComponent, PhysicsComponent, VelocityComponent, SpeedComponent>())
                                entity.NetSync(SpawnPacket.Create(ref entity));
                        }
                        else if (entity.Has<ShapeComponent, PositionComponent, VelocityComponent>())
                            entity.NetSync(ResourceSpawnPacket.Create(ref entity));

                        FConsole.WriteLine($"Spawnpacket sent for {packet.EntityId}");
                        break;
                    }
                case 9000:
                    {
                        var packet = (PingPacket)buffer;
                        var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                        packet.Ping = (ushort)(delta / 10000);

                        player.NetSync(packet);
                        break;
                    }
            }
        }
    }
}