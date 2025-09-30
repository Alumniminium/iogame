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

/// <summary>
/// Central packet processing hub for all incoming client messages.
/// Handles login, input, chat, entity spawning, and component synchronization packets.
/// </summary>
public static class PacketHandler
{
    private static readonly ConcurrentDictionary<PacketId, int> _recvPacketCounts = new();

    /// <summary>
    /// Processes an incoming packet from a player client.
    /// Routes packets to appropriate handlers based on packet ID.
    /// </summary>
    /// <param name="player">The player entity sending the packet</param>
    /// <param name="buffer">Binary packet data buffer</param>
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

                    var inp = new InputComponent(default, default);
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
                        return;

                    if (!NttWorld.EntityExists(packet.Target))
                        return;

                    ref var ntt = ref NttWorld.GetEntity(packet.Target);

                    if (ntt.Has<Box2DBodyComponent>())
                    {
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

    /// <summary>
    /// Handles component state synchronization packets from clients for ship building.
    /// Validates entity ownership and applies component changes securely.
    /// </summary>
    private static void HandleComponentStatePacket(ReadOnlyMemory<byte> buffer, in NTT player)
    {
        var reader = new PacketReader(buffer);
        var targetEntity = reader.ReadNtt();
        var componentType = reader.ReadByte();
        var componentSize = reader.ReadInt16();

        // additional check needed here to prevent client creating entities

        if (!NttWorld.EntityExists(targetEntity))
            NttWorld.CreateEntity(targetEntity);

        var entity = NttWorld.GetEntity(targetEntity);

        // Check if player owns this entity (is a child of player)
        // Skip ownership check for ParentChild packets (they establish ownership)
        if ((ComponentType)componentType != ComponentType.ParentChild)
        {
            if (entity.Has<ParentChildComponent>())
            {
                var parentChild = entity.Get<ParentChildComponent>();
                if (parentChild.ParentId != player) // player tries to modify entity not owned by player -> Hacking attempt
                {
                    Console.WriteLine($"[PacketHandler] REJECTED: Entity {targetEntity.Id} parent is {parentChild.ParentId.Id}, player is {player.Id}");
                    return;
                }
            }
            else if (targetEntity != player) // player tries to modify another entity -> Hacking attempt
            {
                Console.WriteLine($"[PacketHandler] REJECTED: Entity {targetEntity.Id} has no parent and is not player {player.Id}");
                return;
            }
        }

        switch ((ComponentType)componentType)
        {
            case ComponentType.ShipPart:
                {
                    var changedTick = reader.ReadInt64();
                    var gridX = reader.ReadSByte();
                    var gridY = reader.ReadSByte();
                    var type = reader.ReadByte();
                    var shape = reader.ReadByte();
                    var rotation = reader.ReadByte();

                    // ShipPart data now stored in ParentChildComponent
                    if (!entity.Has<ParentChildComponent>())
                        return;

                    ref var parentChild = ref entity.Get<ParentChildComponent>();
                    if (parentChild.ParentId != player)
                        return;

                    parentChild.GridX = gridX;
                    parentChild.GridY = gridY;
                    parentChild.Shape = shape;
                    parentChild.Rotation = rotation;
                    parentChild.ChangedTick = NttWorld.Tick;
                    break;
                }
            case ComponentType.ParentChild:
                {
                    var changedTick = reader.ReadInt64();
                    var parentId = reader.ReadNtt();

                    if (parentId != player)
                        return;

                    var parentChildComponent = new ParentChildComponent(parentId);
                    entity.Set(ref parentChildComponent);
                    break;
                }
            case ComponentType.DeathTag:
                {
                    reader.ReadInt64(); // changedTick (unused)
                    var killerId = reader.ReadNtt();

                    var deathTagComponent = new DeathTagComponent(killerId);
                    entity.Set(ref deathTagComponent);
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
                    var mouseDirX = reader.ReadFloat();
                    var mouseDirY = reader.ReadFloat();
                    var buttonStates = (PlayerInput)reader.ReadUInt16();
                    var didBoostLastFrame = reader.ReadByte() != 0;

                    // Apply input to the validated entity (should be player after ownership check)
                    ref var inp = ref entity.Get<InputComponent>();
                    inp.ButtonStates = buttonStates;
                    inp.MouseDir = new Vector2(mouseDirX, mouseDirY);
                    break;
                }
            case ComponentType.Engine:
                {
                    var changedTick = reader.ReadInt64();
                    var powerUse = reader.ReadFloat();
                    var throttle = reader.ReadFloat();
                    var maxThrustNewtons = reader.ReadFloat();
                    var rcs = reader.ReadByte() != 0;

                    // Check ownership
                    if (!entity.Has<ParentChildComponent>())
                        return;

                    ref readonly var parentChild = ref entity.Get<ParentChildComponent>();
                    if (parentChild.ParentId != player)
                        return;

                    var engineComponent = new EngineComponent(maxThrustNewtons)
                    {
                        PowerUse = powerUse,
                        Throttle = throttle,
                        RCS = rcs
                    };
                    entity.Set(ref engineComponent);
                    break;
                }
            case ComponentType.Shield:
                {
                    var changedTick = reader.ReadInt64();
                    reader.ReadByte(); // powerOn
                    reader.ReadByte(); // lastPowerOn
                    var charge = reader.ReadFloat();
                    var maxCharge = reader.ReadFloat();
                    var powerUse = reader.ReadFloat();
                    var powerUseRecharge = reader.ReadFloat();
                    var radius = reader.ReadFloat();
                    var minRadius = reader.ReadFloat();
                    var targetRadius = reader.ReadFloat();
                    var rechargeRate = reader.ReadFloat();
                    reader.ReadInt64(); // rechargeDelayTicks
                    reader.ReadInt64(); // lastDamageTimeTicks

                    // Check ownership
                    if (!entity.Has<ParentChildComponent>())
                        return;

                    ref readonly var parentChild = ref entity.Get<ParentChildComponent>();
                    if (parentChild.ParentId != player)
                        return;

                    var shieldComponent = new ShieldComponent(
                        charge,
                        maxCharge,
                        powerUse,
                        radius,
                        minRadius,
                        rechargeRate,
                        TimeSpan.FromMilliseconds(500)
                    );
                    entity.Set(ref shieldComponent);
                    break;
                }
            case ComponentType.Weapon:
                {
                    var changedTick = reader.ReadInt64();
                    reader.ReadNtt(); // owner
                    reader.ReadByte(); // fire
                    reader.ReadInt64(); // frequency ticks
                    reader.ReadInt64(); // lastShot ticks
                    var bulletDamage = reader.ReadUInt16();
                    var bulletCount = reader.ReadByte();
                    var bulletSize = reader.ReadByte();
                    var bulletSpeed = reader.ReadUInt16();
                    var powerUse = reader.ReadFloat();
                    reader.ReadFloat(); // directionX
                    reader.ReadFloat(); // directionY

                    // Check ownership
                    if (!entity.Has<ParentChildComponent>())
                        return;

                    ref readonly var parentChild = ref entity.Get<ParentChildComponent>();
                    if (parentChild.ParentId != player)
                        return;

                    var weaponComponent = new WeaponComponent(
                        player,
                        0f, // direction will be calculated from mouse
                        (byte)bulletDamage,
                        bulletCount,
                        bulletSize,
                        (byte)bulletSpeed,
                        powerUse,
                        TimeSpan.FromMilliseconds(1000.0 / 5.0) // Default 5 RPS
                    );
                    entity.Set(ref weaponComponent);
                    break;
                }
            default:
                Console.WriteLine($"[PacketHandler] Unknown component type: {componentType} for entity {targetEntity.Id}");
                break;
        }
    }
}