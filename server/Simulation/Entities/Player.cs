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
            Size = 200;
            MaxSpeed = 1500;
            Drag = 0.999f;
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }

        public string Password { get; internal set; }

        public override void Update(float deltaTime)
        {
            var inputVector = new Vector2(0, 0);
            if (Left)
                inputVector.X -= 1000;
            else if (Right)
                inputVector.X += 1000;

            if (Up)
                inputVector.Y -= 1000;
            else if (Down)
                inputVector.Y += 1000;

            inputVector = inputVector.ClampMagnitude(1000);
            // inputVector *= Speed;

            Velocity += inputVector * deltaTime;

            if (Health < MaxHealth)
            {
                var healthAdd = 10 * deltaTime;
                if (Health + healthAdd > MaxHealth)
                    Health = MaxHealth;
                else
                    Health += healthAdd;
            }

            base.Update(deltaTime);
        }
        internal void AddMovement(uint ticks, bool up, bool down, bool left, bool right)
        {
            var idx = ticks % 5;
            var tickedInput = new TickedInput(ticks, up, down, left, right);
            TickedInputs[idx] = tickedInput;
        }

        public async Task Send(byte[] buffer)
        {
            //TODO: Optimize allocations (ArraySegment is a readonly struct, low priority optimization)
            var arraySegment = new ArraySegment<byte>(buffer);
            await Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
        }
        public async Task ReceiveLoop()
        {
            var result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer), CancellationToken.None).ConfigureAwait(false);
            while (!result.CloseStatus.HasValue)
            {
                try
                {
                    var recvCount = result.Count;

                    while (recvCount < 2)
                    {
                        result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer, recvCount, RecvBuffer.Length - recvCount), CancellationToken.None).ConfigureAwait(false);
                        recvCount += result.Count;
                    }

                    var size = BitConverter.ToUInt16(RecvBuffer, 0);

                    if (size > RecvBuffer.Length || size == 0)
                        break;

                    while (recvCount < size)
                    {
                        result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer, recvCount, size), CancellationToken.None).ConfigureAwait(false);
                        recvCount += result.Count;
                    }
                    var packet = new byte[size];
                    Array.Copy(RecvBuffer, 0, packet, 0, size);

                    await PacketHandler.Handle(this, packet);
                    result = await Socket.ReceiveAsync(new ArraySegment<byte>(RecvBuffer), CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    break;
                }
            }
            if (result.CloseStatus == null)
                await Socket.CloseAsync(WebSocketCloseStatus.ProtocolError, "bullshit packet", CancellationToken.None).ConfigureAwait(false);
            else
                await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
