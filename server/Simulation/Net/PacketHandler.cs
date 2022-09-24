using System;
using System.Runtime.InteropServices;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;
using server.Simulation.Net.Packets;

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
                        var packet = (LoginRequestPacket)buffer;
                        var ntc = new NameTagComponent(packet.GetUsername());

                        var inp = new InputComponent();
                        var eng = new EngineComponent((ushort)(ntc.Name == "trbl" ? 200 : 20));
                        var nrg = new EnergyComponent(100, 500, 1000);
                        var hlt = new HealthComponent(1000, 1000);
                        var reg = new HealthRegenComponent(10);
                        var phy = PhysicsComponent.CreateCircleBody(1, SpawnManager.PlayerSpawnPoint, 1, 1f, Convert.ToUInt32("80ED99", 16));
                        var shi = new ShieldComponent(250, 250, 75, 2, phy.Radius * 1.25f, 5, TimeSpan.FromSeconds(3));
                        var vwp = new ViewportComponent(150);
                        var syn = new NetSyncComponent(SyncThings.All);
                        var wep = new WeaponComponent(0f, 5, 1, 1, 150, 50, TimeSpan.FromMilliseconds(350));
                        var inv = new InventoryComponent(100);
                        var lvl = new LevelComponent(1, 0, 100);

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

                        Game.Grid.Add(in player, ref phy);

                        player.NetSync(LoginResponsePacket.Create(player));
                        PixelWorld.Players.Add(player);
                        Game.Broadcast(SpawnPacket.Create(in player));
                        Game.Broadcast(AssociateIdPacket.Create(player.Id, packet.GetUsername()));
                        Game.Broadcast(ChatPacket.Create(0, $"{packet.GetUsername()} joined!"));
                        foreach (var otherPlayer in PixelWorld.Players)
                        {
                            ref readonly var oNtc = ref otherPlayer.Get<NameTagComponent>();
                            player.NetSync(AssociateIdPacket.Create(otherPlayer.Id, oNtc.Name));
                        }
                        FConsole.WriteLine($"Login Request for User: {packet.GetUsername()}, Pass: {packet.GetPassword()}");
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

                            if (ntt.Type == EntityType.Passive)
                                player.NetSync(SpawnPacket.Create(in ntt));
                            else
                                player.NetSync(SpawnPacket.Create(in ntt));
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