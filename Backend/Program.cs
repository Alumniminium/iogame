using System.Net.WebSockets;
using System.Text;
using iogame.Net;
using iogame.Net.Packets;
using iogame.Simulation;
using iogame.Simulation.Entities;

var game = new Game();
game.Start();

var app = WebApplication.Create(args);

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseWebSockets();

app.MapGet("/chat", async (context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    
    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var player = new Player(webSocket); 
    game.AddPlayer(player);

    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        PacketHandler.Handle(player,buffer);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    game.RemovePlayer(player);
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
});

app.Run();
