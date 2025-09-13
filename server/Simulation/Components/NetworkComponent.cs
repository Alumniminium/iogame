using System;
using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components;

[Component]
public readonly struct NetworkComponent(int entityId, WebSocket socket)
{
    public readonly int EntityId = entityId;
    public readonly WebSocket Socket = socket;
    public readonly Memory<byte> RecvBuffer = new byte[1024 * 4];

    public override int GetHashCode() => EntityId;
}