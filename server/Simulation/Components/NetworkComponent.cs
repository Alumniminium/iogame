using System;
using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct NetworkComponent(WebSocket socket)
{
    public readonly WebSocket Socket = socket;
    public readonly Memory<byte> RecvBuffer = new byte[1024 * 4];


}