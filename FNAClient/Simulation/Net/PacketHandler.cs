using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Packets;
using Packets.Enums;
using RG351MP;
using RG351MP.Helpers;
using RG351MP.Scenes;
using RG351MP.Simulation.Net;

namespace server.Simulation.Net
{
    public static class PacketHandler
    {
        public static void Process(in Memory<byte> buffer)
        {
            try
            {
                if(buffer.Length < 2)
                    return;
                    
                var id = MemoryMarshal.Read<PacketId>(buffer.Span[2..]);

                switch (id)
                {
                    case PacketId.LoginResponse:
                        {
                            LoginResponsePacket packet = buffer;
                            GameScene.Player = new Player(packet.UniqueId, ShapeType.Circle, new Vector2(packet.Position.X, packet.Position.Y),0,0,0, packet.PlayerColor);
                            GameScene.MapSize = new Vector2(packet.MapWidth, packet.MapHeight);
                            GameScene.Entities.TryAdd(packet.UniqueId, GameScene.Player);
                            var viewDistaance = packet.ViewDistance;
                            Console.WriteLine($"Login response");
                            NetClient.LoggedIn = true;
                            break;
                        }
                    // case PacketId.AssociateId:
                    //     {
                    //         AssociateIdPacket packet = buffer;

                    //         break;
                    //     }
                    case PacketId.StatusPacket:
                        {
                            StatusPacket packet = buffer;

                            if (packet.Type == StatusType.Alive)
                            {
                                if (packet.Value == 0)
                                {
                                    GameScene.Entities.Remove(packet.UniqueId);
                                }
                            }
                            break;
                        }
                    // case PacketId.ChatPacket:
                    //     {
                    //         ChatPacket packet = buffer;
                    //         var msg = packet.GetText();
                    //         var sender = packet.UserId;

                    //         break;
                    //     }
                    case PacketId.MovePacket:
                        {
                            MovementPacket packet = buffer;
                            if (packet.UniqueId == GameScene.Player.UniqueId)
                            {
                                GameScene.Player.Position = new Vector2(packet.Position.X, packet.Position.Y);
                                GameScene.Player.direction = packet.Rotation;
                            }
                            else
                            {
                                if (GameScene.Entities.TryGetValue(packet.UniqueId, out Entity value))
                                {
                                    value.Position = new Vector2(packet.Position.X, packet.Position.Y);
                                    value.direction = packet.Rotation;
                                }
                            }
                            break;
                        }
                    case PacketId.CustomSpawnPacket:
                        {
                            SpawnPacket packet = buffer;
                            var entity = new Entity(packet.UniqueId, packet.ShapeType, new Vector2(packet.Position.X, packet.Position.Y), packet.Width, packet.Height, packet.Rotation, packet.Color);
                            
                            if(packet.UniqueId == GameScene.Player.UniqueId)
                            {
                                GameScene.Player.width = packet.Width;
                                GameScene.Player.height = packet.Height;
                                GameScene.Player.direction = packet.Rotation;
                                GameScene.Player.Polygon = new PolygonHelper.Polygon(PolygonHelper.GenerateShape(ShapeType.Box, GameScene.Player.width, GameScene.Player.height, ColorExt.ToColor(packet.Color), packet.Rotation));
                            }
                            else
                                GameScene.Entities.TryAdd(packet.UniqueId, entity);
                            break;
                        }
                    // case PacketId.LineSpawnPacket:
                    //     {
                    //         RayPacket packet = buffer;
                    //         break;
                    //     }
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
