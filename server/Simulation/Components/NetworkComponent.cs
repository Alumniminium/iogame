using System.Net.WebSockets;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct NetworkComponent
    {
        public WebSocket Socket;
        public byte[] RecvBuffer = new byte[1024 * 4];

    }
}