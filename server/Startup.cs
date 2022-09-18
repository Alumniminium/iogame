using System;
using System.Buffers;
using System.Net;
using System.Net.WebSockets;
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
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            Db.CreateResources();
            FConsole.WriteLine($"starting game with tickrate {Game.TargetTps}");
            Game.Broadcast(ChatPacket.Create("Server", "This initializes the Game class"));

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
        public async Task ReceiveLoopAsync(PixelEntity player)
        {
            try
            {
                var net = player.Get<NetworkComponent>();
                var result = await net.Socket.ReceiveAsync(new ArraySegment<byte>(net.RecvBuffer), CancellationToken.None).ConfigureAwait(false);
                while (!result.CloseStatus.HasValue)
                {
                    try
                    {
                        var recvCount = result.Count;

                        while (recvCount < 4) // Receive more until we have the header
                        {
                            FConsole.WriteLine("Got less than 4 bytes");
                            result = await net.Socket.ReceiveAsync(new ArraySegment<byte>(net.RecvBuffer, recvCount, net.RecvBuffer.Length - recvCount), CancellationToken.None).ConfigureAwait(false);
                            recvCount += result.Count;
                        }

                        var size = BitConverter.ToUInt16(net.RecvBuffer, 0);

                        if (size > net.RecvBuffer.Length || size == 0) // packet is malformed, stop and disconnect client
                        {
                            FConsole.WriteLine("Got malformed packet");
                            break;
                        }

                        while (recvCount < size) // receive more bytes until packet is complete
                        {
                            FConsole.WriteLine("Got less than needed");
                            result = await net.Socket.ReceiveAsync(new ArraySegment<byte>(net.RecvBuffer, recvCount, size), CancellationToken.None).ConfigureAwait(false);
                            recvCount += result.Count;
                        }

                        var packet = ArrayPool<byte>.Shared.Rent(size);                      // Create copy of the buffer to work with
                        Array.Copy(net.RecvBuffer, 0, packet, 0, size);  // in case we end up modifying the packet and sending it again

                        IncomingPacketQueue.Add(player, packet);

                        if (recvCount > size) // we got more than we want.
                        {
                            FConsole.WriteLine("Got more than needed");
                            var bytesLeft = recvCount - size;
                            Array.Copy(net.RecvBuffer, size, net.RecvBuffer, 0, bytesLeft); // overwrite
                            result = await net.Socket.ReceiveAsync(new ArraySegment<byte>(net.RecvBuffer, bytesLeft, net.RecvBuffer.Length - bytesLeft), CancellationToken.None).ConfigureAwait(false); // start receiving again
                        }
                        else
                            result = await net.Socket.ReceiveAsync(new ArraySegment<byte>(net.RecvBuffer), CancellationToken.None).ConfigureAwait(false); // start receiving again
                    }
                    catch
                    {
                        FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
                        break;
                    }
                }
                if (result.CloseStatus == null) // server initiated disconnect
                    await net.Socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "bullshit packet", CancellationToken.None).ConfigureAwait(false);
                else                            // client initiated disconnect
                    await net.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);

                var dtc = new DeathTagComponent();
                player.Add(ref dtc);
            }
            catch
            {
                FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
            }
        }
    }
}