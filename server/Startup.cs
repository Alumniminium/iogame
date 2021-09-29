using System.Net;
using System.Net.WebSockets;
using iogame.Simulation;
using iogame.Simulation.Entities;

public class Startup
{
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
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
                    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

                    var player = new Player(webSocket);
                    Game.AddPlayer(player);

                    await player.ReceiveLoop().ConfigureAwait(false);

                    Game.RemoveEntity(player);
                }
                else
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
                await next().ConfigureAwait(false);
        });
    }
}