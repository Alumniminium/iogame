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
                    var eng = new EngineComponent(ntt, 25f);
                    var nrg = new EnergyComponent(ntt, 10000, 50000, 100000);
                    var hlt = new HealthComponent(ntt, 1000, 1000);
                    var reg = new HealthRegenComponent(ntt, 10);
                    var spawnPos = new Vector2(Game.MapSize.X / 2 - 20, Game.MapSize.Y - 5); // Ground level spawn, offset to side
                    // Player uses a unique negative group index - entities with same negative group don't collide
                    int playerGroup = -(Math.Abs(ntt.Id.GetHashCode()) % 1000 + 1); // Ensure negative, avoid 0
                    uint playerCategory = (uint)server.Enums.CollisionCategory.Player;
                    uint playerMask = (uint)server.Enums.CollisionCategory.All;
                    Console.WriteLine($"🎮 Player {ntt.Id} - Group: {playerGroup}, Category: 0x{playerCategory:X4}");
                    var bodyId = Box2DPhysicsWorld.CreateBoxBody(spawnPos, -MathF.PI / 2f, false, 1f, 0.1f, 0.2f, playerCategory, playerMask, playerGroup, true); // Box pointing up (-90°), enable sensor events
                    var box2DBody = new Box2DBodyComponent(ntt, bodyId, false, 0xFF0000, ShapeType.Box, 1f);
                    // Position is accessed directly from Box2D
                    var shi = new ShieldComponent(ntt, 250, 250, 75, 2, 1f * 2f, 5, TimeSpan.FromSeconds(3));
                    var vwp = new ViewportComponent(ntt, 50);
                    var syn = new NetSyncComponent(ntt, SyncThings.All);
                    var wep = new WeaponComponent(ntt, 0f, 5, 1, 1, 30, 50, TimeSpan.FromMilliseconds(350)); // Reduced speed from 150 to 30
                    var inv = new InventoryComponent(ntt, 100);
                    var lvl = new LevelComponent(ntt, 1, 0, 100);

                    player.Set(ref inv);
                    player.Set(ref inp);
                    player.Set(ref eng);
                    player.Set(ref hlt);
                    player.Set(ref reg);
                    player.Set(ref box2DBody);
                    player.Set(ref vwp);
                    player.Set(ref wep);
                    player.Set(ref syn);
                    player.Set(ref nrg);
                    player.Set(ref shi);
                    player.Set(ref ntc);
                    player.Set(ref lvl);

                    player.NetSync(LoginResponsePacket.Create(player, NttWorld.Tick, box2DBody.Position, (int)Game.MapSize.X, (int)Game.MapSize.Y, (ushort)vwp.Viewport.Width, Convert.ToUInt32("80ED99", 16)));
                    NttWorld.Players.Add(player);
                    Game.Broadcast(SpawnPacket.Create(player, box2DBody.ShapeType, box2DBody.Position, box2DBody.Rotation, Convert.ToUInt32("80ED99", 16)));
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

                    if (ntt.Has<Box2DBodyComponent>())
                    {
                        ref readonly var body = ref ntt.Get<Box2DBodyComponent>();
                        // For now, assume box shape with default dimensions
                        player.NetSync(SpawnPacket.Create(ntt, body.ShapeType, body.Position, body.Rotation, body.Color));
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