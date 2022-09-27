using System;
using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct NetworkComponent
    {
        public readonly int EntityId;
        public readonly WebSocket Socket;
        public readonly Memory<byte> RecvBuffer;

        public NetworkComponent(int entityId, WebSocket socket)
        {
            EntityId = entityId;
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }
        public override int GetHashCode() => EntityId;
    }
}