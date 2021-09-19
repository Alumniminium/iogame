using System;
using System.Net.WebSockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace iogame.Simulation.Entities
{
    public class Player : Entity
    {
        public string Name;

        public bool Up, Left, Right, Down;
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
            // Console.WriteLine($"player pos: x={Position.X} y={Position.Y}, vel: x=${Velocity.X} y={Velocity.Y}");
        }

        public void Send(byte[] buffer)
        {
            var arraySegment = new ArraySegment<byte>(buffer);
            Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}
