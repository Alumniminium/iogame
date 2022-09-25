using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using RG351MP.Scenes;
using RG351MP.Simulation.Net;
using server.Helpers;
using server.Simulation.Database;
using server.Simulation.Net.Packets;

namespace server.Simulation.Net
{
    public static class PacketHandler
    {
        public static void Process(in Memory<byte> buffer)
        {
            var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);
            FConsole.WriteLine($"{id}");

            switch (id)
            {
                case PacketId.LoginResponse:
                    {
                        LoginResponsePacket packet = buffer;
                        GameScene.Player= new Player(packet.UniqueId, ShapeType.Circle, packet.Position, packet.PlayerSize,packet.PlayerSize, 0f, 0);
                        GameScene.MapSize = new Vector2(packet.MapWidth, packet.MapHeight);
                        GameScene.Entities.TryAdd(packet.UniqueId, GameScene.Player);
                        Console.WriteLine($"Login response");
                        NetClient.LoggedIn = true;
                        break;
                    }
                case PacketId.AssociateId:
                    {
                        AssociateIdPacket packet = buffer;

                        break;
                    }
                case PacketId.StatusPacket:
                    {
                        StatusPacket packet = buffer;
                        
                        if(packet.Type == StatusType.Alive)
                        {
                            if(packet.Value == 0)
                            {
                                GameScene.Entities.Remove(packet.UniqueId);
                            }
                        }
                        break;
                    }
                case PacketId.ChatPacket:
                    {
                        ChatPacket packet = buffer;
                        var msg = packet.GetText();
                        var sender = packet.UserId;

                        break;
                    }
                case PacketId.MovePacket:
                    {
                        MovementPacket packet = buffer;
                        if (packet.UniqueId == GameScene.Player.UniqueId)
                            GameScene.Player.Position = packet.Position;
                        else
                        {
                            if(GameScene.Entities.TryGetValue(packet.UniqueId, out Entity value))
                                value.Position = packet.Position;
                        }
                        FConsole.WriteLine($"Move packet {packet.UniqueId} {packet.Position}");
                        break;
                    }
                case PacketId.CustomSpawnPacket:
                    {
                        SpawnPacket packet = buffer;
                        var entity = new Entity(packet.UniqueId, packet.ShapeType, packet.Position, packet.Width, packet.Height, packet.Direction, packet.Color);
                        GameScene.Entities.TryAdd(packet.UniqueId, entity);
                        break;
                    }
                case PacketId.LineSpawnPacket:
                    {
                        RayPacket packet = buffer;
                        break;
                    }
                case PacketId.Ping:
                    {
                        PingPacket packet = buffer;
                        if (packet.Ping == 0)
                            NetClient.Send(packet);
                        break;
                    }
                default:
                    Console.WriteLine($"Unknown packet: {id}");
                    break;
            }
        }
    }
}
