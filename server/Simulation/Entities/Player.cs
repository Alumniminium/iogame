using System.Net.WebSockets;
using DefaultEcs;
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
        public ref NetworkComponent NetworkComponent => ref Entity.Get<NetworkComponent>();
        public TickedInput[] TickedInputs = new TickedInput[5];

        public Player(WebSocket socket)
        {
            Entity = PixelWorld.CreateEntity(IdGenerator.Get<Player>());
            NetworkComponent.Socket = socket;
            NetworkComponent.RecvBuffer = new byte[4*1024];
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
            if (NetworkComponent.Socket.State == WebSocketState.Open)
            {
                var arraySegment = new ArraySegment<byte>(buffer, 0, count);
                await NetworkComponent.Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            else
                Disconnect();
        }

        public void Disconnect()
        {
            OutgoingPacketQueue.Remove(this);
            PixelWorld.Destroy(Entity.EntityId);
        }
    }
}