using System.Diagnostics.Tracing;
using System.Net.WebSockets;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Entities
{
    public class Player : ShapeEntity
    {
        public string Name = "Unnamed";
        public string Password = "";
        public ref InputComponent InputComponent => ref Entity.Get<InputComponent>();
        public TickedInput[] TickedInputs = new TickedInput[5];
        public Dictionary<uint, Vector2> LastEntityPositions = new();
        public WebSocket Socket;
        public byte[] RecvBuffer;

        public Player(WebSocket socket)
        {
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
            VIEW_DISTANCE = 1000;
            Viewport = new PlayerScreen(this); 
        }

        internal void AddMovement(uint ticks, bool up, bool down, bool left, bool right)
        {
            var idx = ticks % TickedInputs.Length;
            var tickedInput = new TickedInput(ticks, up, down, left, right);
            TickedInputs[idx] = tickedInput;
        }

        public void Send(byte[] buffer) => OutgoingPacketQueue.Add(this, buffer);
        public async Task ForceSendAsync(byte[] buffer, int count)
        {
            if (Socket.State == WebSocketState.Open)
            {
                var arraySegment = new ArraySegment<byte>(buffer, 0, count);
                await Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            else
                Disconnect();
        }

        public void Disconnect()
        {
            OutgoingPacketQueue.Remove(this);
            World.Destroy(Entity.EntityId);
        }
    }
}