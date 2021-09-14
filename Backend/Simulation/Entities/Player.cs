using System.Net.WebSockets;

namespace iogame.Simulation.Entities
{
    public class Player : Entity
    {
        public string Name;
        public WebSocket Socket;

        public Player(WebSocket socket)
        {
            Size = 30;
            Socket=socket;
        }

        public string Password { get; internal set; }

        public override void Update(float deltaTime)
        {
            // get input from websockets

            if(Health < MaxHealth)
                Health += 10 * deltaTime;

            Velocity *= 0.95f;

            base.Update(deltaTime);
            Position += Velocity * deltaTime;
            // Console.WriteLine($"player pos: x={Position.X} y={Position.Y}, vel: x=${Velocity.X} y={Velocity.Y}");
        }

        public void Send(byte[] buffer)
        {
            var arraySegment = new ArraySegment<byte>(buffer);
            Socket.SendAsync(arraySegment,WebSocketMessageType.Binary,true,CancellationToken.None);
        }
    }
}