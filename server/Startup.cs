using System;
using System.Buffers;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using server.ECS;
using server.Helpers;
using server.Simulation;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Net.Packets;

namespace server
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            Db.CreateResources();
            FConsole.WriteLine($"starting game with tickrate {Game.TargetTps}");
            Game.Broadcast(ChatPacket.Create(0, "This initializes the Game class"));

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                        var ntt = PixelWorld.CreateEntity(EntityType.Player);
                        var net = new NetworkComponent(webSocket);
                        ntt.Add(ref net);
                        await ReceiveLoopAsync(ntt).ConfigureAwait(false);
                    }
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else if (context.Request.Path == "/BaseResources.json")
                    await context.Response.SendFileAsync("BaseResources.json").ConfigureAwait(false);
                else
                    await next().ConfigureAwait(false);
            });
        }
        public async ValueTask ReceiveLoopAsync(PixelEntity player)
        {
            try
            {
                var net = player.Get<NetworkComponent>();
                var result = await net.Socket.ReceiveAsync(net.RecvBuffer, CancellationToken.None).ConfigureAwait(false);

                while (result.Count != 0)
                {
                    try
                    {
                        var recvCount = result.Count;

                        while (recvCount < 4) // Receive more until we have the header
                        {
                            FConsole.WriteLine("Got less than 4 bytes");
                            result = await net.Socket.ReceiveAsync(net.RecvBuffer[recvCount..], CancellationToken.None).ConfigureAwait(false);
                            recvCount += result.Count;
                        }

                        var size = MemoryMarshal.Read<ushort>(net.RecvBuffer.Span);

                        if (size > net.RecvBuffer.Length || size == 0) // packet is malformed, stop and disconnect client
                        {
                            FConsole.WriteLine("Got malformed packet");
                            break;
                        }

                        while (recvCount < size) // recei>ve more bytes until packet is complete
                        {
                            FConsole.WriteLine("Got less than needed");
                            result = await net.Socket.ReceiveAsync(net.RecvBuffer.Slice(recvCount, size), CancellationToken.None).ConfigureAwait(false);
                            recvCount += result.Count;
                        }

                        Memory<byte> packet = new byte[size];
                        net.RecvBuffer[..size].CopyTo(packet);
                        IncomingPacketQueue.Add(in player, in packet);

                        if (recvCount > size) // we got more than we want.
                        {
                            FConsole.WriteLine("Got more than needed");
                            var bytesLeft = recvCount - size;
                            net.RecvBuffer.Slice(size, bytesLeft).CopyTo(net.RecvBuffer);
                            result = await net.Socket.ReceiveAsync(net.RecvBuffer[bytesLeft..], CancellationToken.None).ConfigureAwait(false); // start receiving again
                        }
                        else
                            result = await net.Socket.ReceiveAsync(net.RecvBuffer, CancellationToken.None).ConfigureAwait(false); // start receiving again
                    }
                    catch (Exception e)
                    {
                        player.Add<DeathTagComponent>();
                        FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
                        break;
                    }
                }
                player.Add<DeathTagComponent>();
            }
            catch
            {
                FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
            }
        }
    }
}