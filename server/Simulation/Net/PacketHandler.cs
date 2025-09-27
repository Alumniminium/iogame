using System;
using System.Collections.Generic;
using System.Linq;
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
                    var packet = LoginRequestPacket.Read(buffer);
                    var ntc = new NameTagComponent(ntt, packet.Username);

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
                    // Create default single part for new player
                    var defaultParts = new List<ShipPart> { new(0, 0, 0, (byte)box2DBody.ShapeType, 0) };
                    Game.Broadcast(SpawnPacket.Create(player, box2DBody.ShapeType, box2DBody.Position, box2DBody.Rotation, Convert.ToUInt32("80ED99", 16), defaultParts).ToBuffer());
                    Game.Broadcast(AssociateIdPacket.Create(player, packet.Username));
                    Game.Broadcast(ChatPacket.Create(default, $"{packet.Username} joined!"));
                    foreach (var otherPlayer in NttWorld.Players)
                    {
                        ref readonly var oNtc = ref otherPlayer.Get<NameTagComponent>();
                        player.NetSync(AssociateIdPacket.Create(otherPlayer, oNtc.Name));
                    }
                    LeaderBoard.Broadcast();
                    break;
                }
            case PacketId.ChatPacket:
                {
                    var packet = ChatPacket.Read(buffer);
                    var message = packet.Message;

                    Game.Broadcast(ChatPacket.Create(packet.UserId, message));
                    break;
                }
            case PacketId.InputPacket:
                {
                    var packet = PlayerMovementPacket.Read(buffer);

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
                    var packet = RequestSpawnPacket.Read(buffer);

                    if (player.Id != packet.Requester)
                        return; //hax

                    if (!NttWorld.EntityExists(packet.Target))
                        return;

                    ref var ntt = ref NttWorld.GetEntity(packet.Target);

                    if (ntt.Has<Box2DBodyComponent>())
                    {
                        ref readonly var body = ref ntt.Get<Box2DBodyComponent>();
                        // Create default single part for entity
                        var defaultParts = new List<ShipPart> { new(0, 0, 0, (byte)body.ShapeType, 0) };
                        player.NetSync(SpawnPacket.Create(ntt, body.ShapeType, body.Position, body.Rotation, body.Color, defaultParts).ToBuffer());
                    }

                    break;
                }
            case PacketId.ShipConfiguration:
                {
                    var packet = ShipConfigurationPacket.FromBuffer(buffer);

                    // Validate that the player is configuring their own ship
                    if (player != packet.NTT)
                        return;

                    // Convert parts to Box2D shapes with grid offsets and rotations
                    var shapes = new List<(Vector2 offset, ShapeType shapeType, float shapeRotation)>();
                    foreach (var part in packet.Parts)
                    {
                        var gridVector = new Vector2(part.GridX, part.GridY);
                        var shapeType = part.Shape == 1 ? ShapeType.Triangle : ShapeType.Box; // triangle=1, square=2
                        var shapeRotation = part.Rotation * MathF.PI / 2f;
                        shapes.Add((gridVector, shapeType, shapeRotation));
                    }

                    ref var currentBody = ref player.Get<Box2DBodyComponent>();
                    var currentPosition = currentBody.Position;
                    var currentRotation = currentBody.Rotation;

                    // Destroy old body
                    Box2DPhysicsWorld.DestroyBody(currentBody.BodyId);

                    // Create new compound body
                    uint playerCategory = (uint)CollisionCategory.Player;
                    uint playerMask = (uint)CollisionCategory.All;
                    int playerGroup = -(Math.Abs(player.Id.GetHashCode()) % 1000 + 1); // Same group as before

                    var (newBodyId, localCenter) = Box2DPhysicsWorld.CreateCompoundBody(
                        currentPosition,
                        currentRotation,
                        false,
                        shapes,
                        1f,
                        0.1f,
                        0.2f,
                        playerCategory,
                        playerMask,
                        playerGroup,
                        true
                    );

                    // Update Box2DBodyComponent with new body
                    currentBody.BodyId = newBodyId;
                    currentBody.LocalCenterOfMass = localCenter;
                    currentBody.ShapeType = ShapeType.Box; // Use Box as default for compound shapes

                    // Store ship configuration
                    var shipConfig = new ShipConfigurationComponent(player, packet.Parts);
                    player.Set(ref shipConfig);


                    // Broadcast the change to other players (they'll see the new compound shape)
                    var parts = packet.Parts.Count > 0 ? packet.Parts : new List<ShipPart> { new(0, 0, 0, (byte)currentBody.ShapeType, 0) };
                    var spawnPacket = SpawnPacket.Create(player, currentBody.ShapeType, currentBody.Position, currentBody.Rotation, Convert.ToUInt32("80ED99", 16), parts);
                    Game.Broadcast(spawnPacket.ToBuffer());
                    break;
                }
            case PacketId.Ping:
                {
                    var packet = PingPacket.Read(buffer);
                    var delta = DateTime.UtcNow.Ticks - packet.TickCounter;

                    var responsePacket = PingPacket.Create();
                    player.NetSync(responsePacket);
                    break;
                }
        }
    }
}