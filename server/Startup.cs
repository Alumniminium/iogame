using System.Net;
using System.Net.WebSockets;
using iogame.Net;
using iogame.Simulation;
using iogame.Simulation.Entities;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        var game = new Game();
        game.Start();

        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/chat")
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var player = new Player(webSocket);
                game.AddPlayer(player);

                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var recvCount = result.Count;

                    while(recvCount < 2)
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer,recvCount,buffer.Length - recvCount),CancellationToken.None);
                        recvCount+=result.Count;
                    }

                    var size = BitConverter.ToUInt16(buffer,0);

                    while(size < recvCount)
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer,recvCount,size),CancellationToken.None);
                        recvCount += result.Count;
                    }

                    PacketHandler.Handle(player, buffer);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                game.RemovePlayer(player);

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            else
                await next();
        });
    }
}