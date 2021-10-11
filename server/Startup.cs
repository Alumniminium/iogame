using System.Net;
using System.Net.WebSockets;
using iogame.Net;
using iogame.Simulation;
using iogame.Simulation.Entities;

namespace iogame
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment _)
        {
            Game.Start();  // This doesn't belong here...
                           // .. right? 
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                        var player = new Player(webSocket);

                        await ReceiveLoop(player);

                        await Game.RemoveEntity(player);
                    }
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else
                    await next();
            });
        }
        public async Task ReceiveLoop(Player player)
        {
            var result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(player.RecvBuffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                try
                {
                    var recvCount = result.Count;

                    while (recvCount < 4) // Receive more until we have the header
                    {
                        result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(player.RecvBuffer, recvCount, player.RecvBuffer.Length - recvCount), CancellationToken.None);
                        recvCount += result.Count;
                    }

                    var size = BitConverter.ToUInt16(player.RecvBuffer, 0);

                    if (size > player.RecvBuffer.Length || size == 0) // packet is malformed, stop and disconnect client
                        break;

                    while (recvCount < size) // receive more bytes until packet is complete
                    {
                        result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(player.RecvBuffer, recvCount, size), CancellationToken.None);
                        recvCount += result.Count;
                    }

                    var packet = new byte[size];                        // Create copy of the buffer to work with
                    Array.Copy(player.RecvBuffer, 0, packet, 0, size);  // in case we end up modifying the packet and sending it again

                    await PacketHandler.Handle(player, packet);

                    if (recvCount > size) // we got more than we want.
                    {
                        var bytesLeft = recvCount - size;
                        Array.Copy(player.RecvBuffer, size, player.RecvBuffer, 0, bytesLeft); // overwrite
                        result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(player.RecvBuffer, bytesLeft, player.RecvBuffer.Length - bytesLeft), CancellationToken.None); // start receiving again
                    }
                    else
                        result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(player.RecvBuffer), CancellationToken.None); // start receiving again
                }
                catch
                {
                    Console.WriteLine("Error"); // something went wrong, stop and disconnect client
                    break;
                }
            }

            if (result.CloseStatus == null) // server initiated disconnect
                await player.Socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "bullshit packet", CancellationToken.None);
            else                            // client initiated disconnect
                await player.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}