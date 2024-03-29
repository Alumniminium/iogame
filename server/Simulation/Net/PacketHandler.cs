using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Packets;
using Packets.Enums;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Net
{
    public static class PacketHandler
    {
        public static void Process(in PixelEntity player, in Memory<byte> buffer)
        {
            var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);

            switch (id)
            {
                case PacketId.LoginRequest:
                    {
                        var ntt = player;
                        var packet = (LoginRequestPacket)buffer;
                        var ntc = new NameTagComponent(ntt.Id, packet.GetUsername());

                        var inp = new InputComponent(ntt.Id, default, default, default);
                        var eng = new EngineComponent(ntt.Id, (ushort)(ntc.Name == "trbl" ? 10 : 5));
                        var nrg = new EnergyComponent(ntt.Id, 10000, 50000, 100000);
                        var hlt = new HealthComponent(ntt.Id, 1000, 1000);
                        var reg = new HealthRegenComponent(ntt.Id, 10);
                        var phy = PhysicsComponent.CreateBoxBody(ntt.Id, 45,4, SpawnManager.PlayerSpawnPoint, 1, 1f, Convert.ToUInt32("80ED99", 16));
                        var shi = new ShieldComponent(ntt.Id, 250, 250, 75, 2, phy.Radius * 2f, 5, TimeSpan.FromSeconds(3));
                        var vwp = new ViewportComponent(ntt.Id, 300);
                        var aabb = new AABBComponent(ntt.Id,new System.Drawing.RectangleF(phy.Position.X- phy.Size/2, phy.Position.Y- phy.Size/2, phy.Size, phy.Size));
                        var syn = new NetSyncComponent(ntt.Id, SyncThings.All);
                        var wep = new WeaponComponent(ntt.Id, 0f, 5, 1, 1, 150, 50, TimeSpan.FromMilliseconds(350));
                        var inv = new InventoryComponent(ntt.Id, 100);
                        var lvl = new LevelComponent(ntt.Id, 1, 0, 100);

                        player.Add(ref inv);
                        player.Add(ref inp);
                        player.Add(ref eng);
                        player.Add(ref hlt);
                        player.Add(ref reg);
                        player.Add(ref phy);
                        player.Add(ref vwp);
                        player.Add(ref wep);
                        player.Add(ref syn);
                        player.Add(ref nrg);
                        player.Add(ref shi);
                        player.Add(ref ntc);
                        player.Add(ref lvl);
                        player.Add(ref aabb);

                        Game.Grid.Add(in player, ref phy);


                        var wing1 = PixelWorld.CreateEntity(EntityType.Passive, player.Id);
                        var wing1Phy = PhysicsComponent.CreateBoxBody(wing1.Id, 40, 3, SpawnManager.PlayerSpawnPoint, 1, 1f, Convert.ToUInt32("80ED99", 16));
                        var wing1syn = new ChildOffsetComponent(wing1.Id, player.Id, new Vector2(0,phy.Width*0.05f), 90f.ToRadians());
                        var wing1aabb = new AABBComponent(wing1.Id, new System.Drawing.RectangleF(wing1Phy.Position.X - wing1Phy.Size / 2, wing1Phy.Position.Y - wing1Phy.Size / 2, wing1Phy.Size, wing1Phy.Size));
                        wing1.Add(ref wing1Phy);
                        wing1.Add(ref wing1syn);
                        wing1.Add(ref syn);
                        wing1.Add(ref wing1aabb);
                        player.AttachChild(wing1);
                        Game.Grid.Add(in wing1, ref wing1Phy);


                        var wing2 = PixelWorld.CreateEntity(EntityType.Passive, player.Id);
                        var wing2Phy = PhysicsComponent.CreateBoxBody(wing2.Id, 20, 3, SpawnManager.PlayerSpawnPoint, 1, 1f, Convert.ToUInt32("80ED99", 16));
                        var wing2syn = new ChildOffsetComponent(wing2.Id, player.Id, new Vector2(0, phy.Width*0.45f), 90f.ToRadians());
                        var wing2aabb = new AABBComponent(wing2.Id, new System.Drawing.RectangleF(wing2Phy.Position.X - wing2Phy.Size / 2, wing2Phy.Position.Y - wing2Phy.Size / 2, wing2Phy.Size, wing2Phy.Size));
                        wing2.Add(ref wing2Phy);
                        wing2.Add(ref wing2syn);
                        wing2.Add(ref syn);
                        wing2.Add(ref wing2aabb);
                        player.AttachChild(wing2);
                        Game.Grid.Add(in wing2, ref wing2Phy);




                        player.NetSync(LoginResponsePacket.Create(player.Id, Game.CurrentTick, phy.Position, (int)Game.MapSize.X, (int)Game.MapSize.Y, (ushort)vwp.Viewport.Width, phy.Color));
                        PixelWorld.Players.Add(player);
                        Game.Broadcast(SpawnPacket.Create(player.Id, phy.ShapeType, phy.Radius, phy.Width,phy.Height,phy.Position,phy.RotationRadians,phy.Color));
                        Game.Broadcast(AssociateIdPacket.Create(player.Id, packet.GetUsername()));
                        Game.Broadcast(ChatPacket.Create(0, $"{packet.GetUsername()} joined!"));
                        foreach (var otherPlayer in PixelWorld.Players)
                        {
                            ref readonly var oNtc = ref otherPlayer.Get<NameTagComponent>();
                            player.NetSync(AssociateIdPacket.Create(otherPlayer.Id, oNtc.Name));
                        }
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
                        LeaderBoard.Broadcast();
                        break;
                    }
                case PacketId.ChatPacket:
                    {
                        var packet = (ChatPacket)buffer;
                        var message = packet.GetText();

                        Game.Broadcast(ChatPacket.Create(packet.UserId, message));
                        FConsole.WriteLine($"ChatPacket from {packet.UserId}: {message}");
                        break;
                    }
                case PacketId.PlayerMovePacket:
                    {
                        var packet = (PlayerMovementPacket)buffer;

                        if (packet.UniqueId != player.Id)
                            return; // hax

                        // var ticks = packet.TickCounter;
                        ref var inp = ref player.Get<InputComponent>();

                        // inp.MovementAxis = movement;
                        inp.ButtonStates = packet.Inputs;
                        inp.MouseDir = packet.MousePosition;
                        break;
                    }
                case PacketId.RequestSpawnPacket:
                    {
                        var packet = (RequestSpawnPacket)buffer;
                        FConsole.WriteLine($"RequestSpawnPacket from {packet.UniqueId} for {packet.EntityId}");

                        if (player.Id != packet.UniqueId)
                            return; //hax

                        if (!PixelWorld.EntityExists(packet.EntityId))
                            return;

                        ref var ntt = ref PixelWorld.GetEntity(packet.EntityId);

                        if (ntt.Has<PhysicsComponent>())
                        {
                            ref readonly var phy = ref ntt.Get<PhysicsComponent>();

                            player.NetSync(SpawnPacket.Create(ntt.Id, phy.ShapeType, phy.Radius, phy.Width, phy.Height, phy.Position, phy.RotationRadians, phy.Color));
                        }

                        FConsole.WriteLine($"Spawnpacket sent for {packet.EntityId}");
                        break;
                    }
                case PacketId.Ping:
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