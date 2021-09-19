using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using iogame.Net;
using iogame.Simulation;
using iogame.Simulation.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace iogame
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var game = new Game();
            game.Start();
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseKestrel(opts =>
                    {
                        opts.ListenAnyIP(5000);
                    });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var player = new Player(webSocket);
                        game.AddPlayer(player);

                        var buffer = new byte[1024 * 4];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        while (!result.CloseStatus.HasValue)
                        {
                            PacketHandler.Handle(player, buffer);
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }
                        game.RemovePlayer(player);
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    await next();
                }

            });

            app.Run();
        }
    }
}