using System.Net.WebSockets;
using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class Player : Entity
    {
        public string Name;

        public bool Up, Left, Right, Down;
        public TickedInput[] TickedInputs = new TickedInput[5];
        public WebSocket Socket;

        public Player(WebSocket socket)
        {
            Size = 30;
            Speed = 10;
            Socket = socket;
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

        public void Send(byte[] buffer)
        {
            //TODO: Optimize allocations (ArraySegment is a readonly struct, low priority optimization)
            var arraySegment = new ArraySegment<byte>(buffer);
            Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        internal void AddMovement(uint ticks, bool up, bool down, bool left, bool right)
        {
            var idx = ticks % 5;
            var tickedInput = new TickedInput(ticks,up,down,left,right);
            TickedInputs[idx] = tickedInput;
        }
    }
}
