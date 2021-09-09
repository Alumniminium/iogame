using System.Net;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

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
            await Echo(context, webSocket);
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

async Task Echo(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);
        bw.Write((short)-1);
        bw.Write((short)1);
        bw.Write(10);
        bw.Write(10);
        bw.Seek(0,SeekOrigin.Begin);
        bw.Write((short)ms.Length);
        var packet = ms.ToArray();
        await webSocket.SendAsync(new ArraySegment<byte>(packet,0,packet.Length),WebSocketMessageType.Binary,true,CancellationToken.None);

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

app.Run();
