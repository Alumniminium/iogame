using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components
{
    [Flags]
    public enum SyncThings : ushort
    {
        None = 0b0000000000000000,
        Position = 0b0000000000000001,
        Health = 0b0000000000000010,
        Size = 0b0000000000000100,
        Viewport = 0b0000000000001000,
        Invenory = 0b0000000000010000,
        Power = 0b0000000000100000,

        All = 0b1111111111111111,
    }
    [Component]
    public struct NetSyncComponent
    {
        public SyncThings Fields = SyncThings.None;

        public NetSyncComponent(SyncThings fields)
        {
            Fields = fields;
        }
    }
    [Component]
    public struct NetworkComponent
    {
        public WebSocket Socket;
        public byte[] RecvBuffer;


        public NetworkComponent(WebSocket socket)
        {
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }
    }
}