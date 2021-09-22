using System.Net.WebSockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Net;

namespace iogame.Simulation.Entities
{
    public class Player : Entity
    {
        public string Name;

        public bool Up, Left, Right, Down;
        public TickedInput[] TickedInputs = new TickedInput[5];
        public WebSocket Socket;
        public byte[] RecvBuffer;

        public Player(WebSocket socket)
        {
            Size = 30;
            Speed = 10;
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }

        public string Password { get; internal set; }

        public override void Update(float deltaTime)
        {
            var inputVector = new Vector2(0, 0);
            if (Left)
                inputVector.X--;
            else if (Right)
                inputVector.X++;

            if (Up)
                inputVector.Y--;
            else if (Down)
                inputVector.Y++;

            inputVector = inputVector.ClampMagnitude(1);
            inputVector *= Speed;

            Velocity += inputVector;

            if (Health < MaxHealth)
                Health += 10 * deltaTime;

            Velocity *= 0.95f;

            base.Update(deltaTime);
        }
        internal void AddMovement(uint ticks, bool up, bool down, bool left, bool right)
        {
            var idx = ticks % 5;
            var tickedInput = new TickedInput(ticks, up, down, left, right);
            TickedInputs[idx] = tickedInput;
        }

        public void Send(byte[] buffer)
        {
            //TODO: Optimize allocations (ArraySegment is a readonly struct, low priority optimization)
            var arraySegment = new ArraySegment<byte>(buffer);
            Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        public async Task ReceiveLoop()
        {
            var result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var recvCount = result.Count;

                while (recvCount < 2)
                {
                    result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer, recvCount, RecvBuffer.Length - recvCount), CancellationToken.None);
                    recvCount += result.Count;
                }

                var size = BitConverter.ToUInt16(RecvBuffer, 0);

                while (size < recvCount)
                {
                    result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer, recvCount, size), CancellationToken.None);
                    recvCount += result.Count;
                }
                var packet = new byte[size];
                Array.Copy(RecvBuffer, 0, packet, 0, size);

                PacketHandler.Handle(this, packet);
                result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer), CancellationToken.None);
            }

            await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
