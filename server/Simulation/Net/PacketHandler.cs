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
        public static void Process(PixelEntity player, Memory<byte> buffer)
        {
            var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);

            switch (id)
            {
                case PacketId.LoginRequest:
                    {
                        var packet = (LoginRequestPacket)buffer;
                        // player.Name = packet.GetUsername();
                        // player.Password = packet.GetPassword();

                        var inp = new InputComponent();
                        var eng = new EngineComponent(200);
                        var nrg = new EnergyComponent(325, 500, 1000);
                        var shi = new ShieldComponent(750, 750, 75, 20, 50);
                        var hlt = new HealthComponent(20000, 20000, 10);
                        var phy = PhysicsComponent.CreateCircleBody(5, SpawnManager.GetPlayerSpawnPoint(), 1, 0.1f, Convert.ToUInt32("80ED99", 16));
                        var vwp = new ViewportComponent(500);
                        var syn = new NetSyncComponent(SyncThings.All);
                        var wep = new WeaponComponent(0f);
                        var inv = new InventoryComponent(100);

                        player.Add(ref inv);
                        player.Add(ref inp);
                        player.Add(ref eng);
                        player.Add(ref hlt);
                        player.Add(ref phy);
                        player.Add(ref vwp);
                        player.Add(ref wep);
                        player.Add(ref syn);
                        player.Add(ref nrg);
                        player.Add(ref shi);

                        Game.Grid.Add(in player, ref phy);

                        player.NetSync(LoginResponsePacket.Create(player));
                        player.NetSync(SpawnPacket.Create(in player));
                        PixelWorld.Players.Add(player);
                        Game.Broadcast(ChatPacket.Create(0, $"{packet.GetUsername()} joined!"));
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