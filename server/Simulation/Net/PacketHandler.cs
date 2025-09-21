using System;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Net;

public static class PacketHandler
{
    public static void Process(in NTT player, in Memory<byte> buffer)
    {
        var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);

        switch (id)
        {
            case PacketId.LoginRequest:
                {
                    var ntt = player;
                    var packet = (LoginRequestPacket)buffer;
                    var ntc = new NameTagComponent(ntt, packet.GetUsername());

                    var inp = new InputComponent(ntt, default, default, default);
                    var eng = new EngineComponent(ntt, (ushort)(ntc.Name == "trbl" ? 10 : 5));
                    var nrg = new EnergyComponent(ntt, 10000, 50000, 100000);
                    var hlt = new HealthComponent(ntt, 1000, 1000);
                    var reg = new HealthRegenComponent(ntt, 10);
                    var phy = PhysicsComponent.CreateCircleBody(22.5f, SpawnManager.PlayerSpawnPoint, 1, 1f, Convert.ToUInt32("80ED99", 16));
                    phy.LinearVelocity = Vector2.One * 10;
                    phy.Acceleration = Vector2.One * 10;
                    var shi = new ShieldComponent(ntt, 250, 250, 75, 2, phy.Radius * 2f, 5, TimeSpan.FromSeconds(3));
                    var vwp = new ViewportComponent(ntt, 3000);
                    var aabb = new AABBComponent(ntt, new System.Drawing.RectangleF(phy.Position.X - phy.Size / 2, phy.Position.Y - phy.Size / 2, phy.Size, phy.Size));
                    var syn = new NetSyncComponent(ntt, SyncThings.All);
                    var wep = new WeaponComponent(ntt, 0f, 5, 1, 1, 150, 50, TimeSpan.FromMilliseconds(350));
                    var inv = new InventoryComponent(ntt, 100);
                    var lvl = new LevelComponent(ntt, 1, 0, 100);

                    player.Set(ref inv);
                    player.Set(ref inp);
                    player.Set(ref eng);
                    player.Set(ref hlt);
                    player.Set(ref reg);
                    player.Set(ref phy);
                    player.Set(ref vwp);
                    player.Set(ref wep);
                    player.Set(ref syn);
                    player.Set(ref nrg);
                    player.Set(ref shi);
                    player.Set(ref ntc);
                    player.Set(ref lvl);
                    player.Set(ref aabb);

                    Game.Grid.Add(in player, ref phy);

                    player.NetSync(LoginResponsePacket.Create(player, NttWorld.Tick, phy.Position, (int)Game.MapSize.X, (int)Game.MapSize.Y, (ushort)vwp.Viewport.Width, phy.Color));
                    NttWorld.Players.Add(player);
                    Game.Broadcast(SpawnPacket.Create(player, phy.ShapeType, phy.Radius, phy.Width, phy.Height, phy.Position, phy.RotationRadians, phy.Color));
                    Game.Broadcast(AssociateIdPacket.Create(player, packet.GetUsername()));
                    Game.Broadcast(ChatPacket.Create(default, $"{packet.GetUsername()} joined!"));
                    foreach (var otherPlayer in NttWorld.Players)
                    {
                        ref readonly var oNtc = ref otherPlayer.Get<NameTagComponent>();
                        player.NetSync(AssociateIdPacket.Create(otherPlayer, oNtc.Name));
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
            case PacketId.InputPacket:
                {
                    var packet = (PlayerMovementPacket)buffer;
                    FConsole.WriteLine($"PlayerMovementPacket from {packet.UniqueId} with {packet.Inputs} inputs");

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
                    FConsole.WriteLine($"RequestSpawnPacket from {packet.Requester} for {packet.Target}");

                    if (player.Id != packet.Requester)
                        return; //hax

                    if (!NttWorld.EntityExists(packet.Target))
                        return;

                    ref var ntt = ref NttWorld.GetEntity(packet.Target);

                    if (ntt.Has<PhysicsComponent>())
                    {
                        ref readonly var phy = ref ntt.Get<PhysicsComponent>();

                        player.NetSync(SpawnPacket.Create(ntt, phy.ShapeType, phy.Radius, phy.Width, phy.Height, phy.Position, phy.RotationRadians, phy.Color));
                    }

                    FConsole.WriteLine($"Spawnpacket sent for {packet.Target}");
                    break;
                }
            case PacketId.Ping:
                {
                    var packet = (PingPacket)buffer;
                    var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                    packet.Ping = (ushort)(delta / 10000);
                    FConsole.WriteLine($"Ping: {packet.Ping / 2000}ms");
                    player.NetSync(packet);
                    break;
                }
        }
    }
}