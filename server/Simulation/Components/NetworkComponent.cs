using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct NetworkComponent
    {
        public WebSocket Socket;
        public byte[] RecvBuffer = new byte[1024 * 4];
    }
}