using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Serialization;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Net;

public static class PacketHandler
{
    private static readonly ConcurrentDictionary<PacketId, int> _recvPacketCounts = new();
    private static long _lastRecvLogTick = 1;

    public static void Process(in NTT player, in Memory<byte> buffer)
    {
        var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);

        switch (id)
        {
            case PacketId.LoginRequest:
                {
                    var ntt = player;
                    var packet = LoginRequestPacket.Read(buffer);
                    var ntc = new NameTagComponent(packet.Username);

                    var inp = new InputComponent(default, default, default);
                    var eng = new EngineComponent(25f);
                    var nrg = new EnergyComponent(10000, 50000, 100000);
                    var hlt = new HealthComponent(1000, 1000);
                    var reg = new HealthRegenComponent(10);
                    var spawnPos = new Vector2(Game.MapSize.X / 2, Game.MapSize.Y / 2);
                    int playerGroup = -(Math.Abs(ntt.Id.GetHashCode()) % 1000 + 1);
                    uint playerCategory = (uint)CollisionCategory.Player;
                    uint playerMask = (uint)CollisionCategory.All;
                    var bodyId = Box2DPhysicsWorld.CreateBoxBody(spawnPos, -MathF.PI / 2f, false, 1f, 0.1f, 0.2f, playerCategory, playerMask, playerGroup, true); // Box pointing up (-90°), enable sensor events
                    var box2DBody = new Box2DBodyComponent(bodyId, false, 0xFF0000, 1f, 4);
                    var shi = new ShieldComponent(250, 250, 75, 5, 2.5f, 5, TimeSpan.FromSeconds(3));
                    var vwp = new ViewportComponent(50);
                    var wep = new WeaponComponent(ntt, 0f, 5, 1, 1, 30, 50, TimeSpan.FromMilliseconds(350)); // Reduced speed from 150 to 30
                    var inv = new InventoryComponent(100);
                    var lvl = new LevelComponent(1, 0, 100);

                    player.Set(ref inv);
                    player.Set(ref inp);
                    player.Set(ref eng);
                    player.Set(ref hlt);
                    player.Set(ref reg);
                    player.Set(ref box2DBody);
                    player.Set(ref vwp);
                    player.Set(ref wep);
                    player.Set(ref nrg);
                    player.Set(ref shi);
                    player.Set(ref ntc);
                    player.Set(ref lvl);

                    player.NetSync(LoginResponsePacket.Create(player, NttWorld.Tick, box2DBody.Position, (int)Game.MapSize.X, (int)Game.MapSize.Y, (ushort)vwp.Viewport.Width, Convert.ToUInt32("80ED99", 16)));
                    NttWorld.Players.Add(player);

                    Game.Broadcast(ChatPacket.Create(default, $"{packet.Username} joined!"));
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
                        // Trigger component sync for the requested entity
                        ref var body = ref ntt.Get<Box2DBodyComponent>();
                        body.ChangedTick = NttWorld.Tick;
                    }

                    break;
                }
            case PacketId.ComponentState:
                {
                    HandleComponentStatePacket(buffer, player);
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

    private static void HandleComponentStatePacket(ReadOnlyMemory<byte> buffer, in NTT player)
    {
        var reader = new PacketReader(buffer);
        var entityId = reader.ReadNtt();
        var componentId = reader.ReadByte();
        var dataLength = reader.ReadInt16();

        // Validate that the entity belongs to the player (security check)
        if (entityId.Has<ParentChildComponent>())
        {
            var parentChild = entityId.Get<ParentChildComponent>();
            if (parentChild.ParentId != player)
            {
                // Entity doesn't belong to this player, ignore
                return;
            }
        }

        // Create entity if it doesn't exist
        // we need to validate that before we launch
        // huge security issue
        if (!NttWorld.EntityExists(entityId))
        {
            NttWorld.CreateEntity(entityId);
        }

        var entity = NttWorld.GetEntity(entityId);

        // Handle different component types
        switch ((ComponentType)componentId)
        {
            case ComponentType.ShipPart:
                {
                    var changedTick = reader.ReadInt64();
                    var gridX = reader.ReadSByte();
                    var gridY = reader.ReadSByte();
                    var type = reader.ReadByte();
                    var shape = reader.ReadByte();
                    var rotation = reader.ReadByte();

                    var shipPartComponent = new ShipPartComponent(gridX, gridY, type, shape, rotation);
                    entity.Set(ref shipPartComponent);
                    break;
                }
            case ComponentType.ParentChild:
                {
                    var changedTick = reader.ReadInt64();
                    var parentId = reader.ReadNtt();

                    // Validate that the parent is the requesting player
                    if (parentId != player)
                    {
                        // Invalid parent, ignore
                        return;
                    }

                    var parentChildComponent = new ParentChildComponent(parentId);
                    entity.Set(ref parentChildComponent);
                    break;
                }
            case ComponentType.DeathTag:
                {
                    var changedTick = reader.ReadInt64();
                    var killerId = reader.ReadNtt();

                    // Validate that the entity being removed belongs to the player
                    if (entity.Has<ParentChildComponent>())
                    {
                        var parentChild = entity.Get<ParentChildComponent>();
                        if (parentChild.ParentId != player)
                        {
                            // Entity doesn't belong to this player, ignore
                            return;
                        }

                        // Add a very short lifetime component so it gets synced to clients before removal
                        var lifetimeComponent = new LifeTimeComponent(TimeSpan.FromMilliseconds(50)); // 50ms delay
                        entity.Set(ref lifetimeComponent);
                    }
                    break;
                }
            case ComponentType.Color:
                {
                    var changedTick = reader.ReadInt64();
                    var color = reader.ReadUInt32();

                    var colorComponent = new ColorComponent(color);
                    entity.Set(ref colorComponent);
                    break;
                }
            case ComponentType.Input:
                {
                    var changedTick = reader.ReadInt64();
                    var movementAxisX = reader.ReadFloat();
                    var movementAxisY = reader.ReadFloat();
                    var mouseDirX = reader.ReadFloat();
                    var mouseDirY = reader.ReadFloat();
                    var buttonStates = (PlayerInput)reader.ReadUInt16();
                    var didBoostLastFrame = reader.ReadByte() != 0;

                    // Validate that the entity is the player
                    if (entityId != player)
                        return; // Security check failed

                    ref var inp = ref player.Get<InputComponent>();
                    inp.ButtonStates = buttonStates;
                    inp.MouseDir = new Vector2(mouseDirX, mouseDirY);
                    break;
                }
            default:
                // Unknown or unsupported component type for client creation
                break;
        }
    }
}