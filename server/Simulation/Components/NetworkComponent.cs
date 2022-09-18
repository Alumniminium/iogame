using System;
using System.Net.WebSockets;
using server.ECS;

namespace server.Simulation.Components
{
    [Flags]
    public enum SyncThings : ushort
    {
        None = 0,
        Position = 1,
        Health = 2,
        Size = 4,
        Viewport = 8,
        Invenory = 16,
        Throttle = 32,
        Battery = 64,
        Shield = 128,

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