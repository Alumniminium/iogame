using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct NetworkComponent
    {
        public readonly WebSocket Socket;
        public readonly byte[] RecvBuffer;

        public NetworkComponent(WebSocket socket)
        {
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }
    }
}