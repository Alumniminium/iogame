using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Database;
using iogame.Simulation.Entities;
using iogame.Util;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;

namespace iogame
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment _)
        {
            Db.CreateResources();
            Console.WriteLine("starting game with tickrate " + Game.TARGET_TPS);
            Game.Broadcast(ChatPacket.Create("Server","Welcome"));

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await ReceiveLoopAsync(new Player(webSocket));
                    }
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else if(context.Request.Path == "/BaseResources.json")
                {
                    await context.Response.SendFileAsync("BaseResources.json");
                }
                else
                    await next();
            });
        }
        public async Task ReceiveLoopAsync(Player player)
        {
            try
            {
                var result = await player.NetworkComponent.Socket.ReceiveAsync(new ArraySegment<byte>(player.NetworkComponent.RecvBuffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    try
                    {
                        var recvCount = result.Count;

                        while (recvCount < 4) // Receive more until we have the header
                        {
                            FConsole.WriteLine("Got less than 4 bytes");
                            result = await player.NetworkComponent.Socket.ReceiveAsync(new ArraySegment<byte>(player.NetworkComponent.RecvBuffer, recvCount, player.NetworkComponent.RecvBuffer.Length - recvCount), CancellationToken.None);
                            recvCount += result.Count;
                        }

                        var size = BitConverter.ToUInt16(player.NetworkComponent.RecvBuffer, 0);

                        if (size > player.NetworkComponent.RecvBuffer.Length || size == 0) // packet is malformed, stop and disconnect client
                        {

                            FConsole.WriteLine("Got malformed packet");
                            break;
                        }

                        while (recvCount < size) // receive more bytes until packet is complete
                        {
                            FConsole.WriteLine("Got less than needed");
                            result = await player.NetworkComponent.Socket.ReceiveAsync(new ArraySegment<byte>(player.NetworkComponent.RecvBuffer, recvCount, size), CancellationToken.None);
                            recvCount += result.Count;
                        }

                        var packet = ArrayPool<byte>.Shared.Rent(size);                      // Create copy of the buffer to work with
                        Array.Copy(player.NetworkComponent.RecvBuffer, 0, packet, 0, size);  // in case we end up modifying the packet and sending it again

                        IncomingPacketQueue.Add(player, packet);
                        //PacketHandler.Process(player, packet);

                        if (recvCount > size) // we got more than we want.
                        {
                            FConsole.WriteLine("Got more than needed");
                            var bytesLeft = recvCount - size;
                            Array.Copy(player.NetworkComponent.RecvBuffer, size, player.NetworkComponent.RecvBuffer, 0, bytesLeft); // overwrite
                            result = await player.NetworkComponent.Socket.ReceiveAsync(new ArraySegment<byte>(player.NetworkComponent.RecvBuffer, bytesLeft, player.NetworkComponent.RecvBuffer.Length - bytesLeft), CancellationToken.None); // start receiving again
                        }
                        else
                            result = await player.NetworkComponent.Socket.ReceiveAsync(new ArraySegment<byte>(player.NetworkComponent.RecvBuffer), CancellationToken.None); // start receiving again
                    }
                    catch
                    {
                        FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
                        break;
                    }
                }
                if (result.CloseStatus == null) // server initiated disconnect
                    await player.NetworkComponent.Socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "bullshit packet", CancellationToken.None);
                else                            // client initiated disconnect
                    await player.NetworkComponent.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                player.Disconnect();
            }
            catch
            {
                FConsole.WriteLine("Error"); // something went wrong, stop and disconnect client
            }
        }
    }
}