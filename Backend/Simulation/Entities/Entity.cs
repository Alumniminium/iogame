using System.Diagnostics;
using System.Net.WebSockets;
using System.Numerics;
using iogame.Net.Packets;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Caching.Memory;

namespace iogame.Simulation.Entities
{
    public class Entity
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Direction;
        public float Size;
        public int Speed;
        public float Health;
        public int MaxHealth;
        public bool InCollision;

        public Vector2 Origin => new(Position.X + Size / 2, Position.Y + Size / 2);


        public virtual void Update(float deltaTime)
        {
            if (float.IsNaN(Velocity.X) || float.IsNaN(Velocity.Y))
                Debugger.Break();
            if (float.IsInfinity(Velocity.X) || float.IsInfinity(Velocity.Y))
                Debugger.Break();
                
            if (!InCollision)
            {
                if (Math.Abs(Velocity.X) > 0.05f)
                {
                    Velocity.X *= 0.999f;
                }
                if (Math.Abs(Velocity.Y) > 0.05f)
                {
                    Velocity.Y *= 0.999f;
                }
            }

            var radians = Math.Atan2(Velocity.X, Velocity.Y);
            Direction = (float)(180 * radians / Math.PI);
            Position += Velocity;
        }
    }

    public class YellowSquare : Entity
    {
        public YellowSquare(float x, float y, float vX, float vY)
        {
            Position = new Vector2(x,y);
            Velocity = new Vector2(vX,vY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
    public class Player : Entity
    {
        public string Name;
        public WebSocket Socket;

        public Player(WebSocket socket)
        {
            Socket=socket;
        }

        public string Password { get; internal set; }

        public override void Update(float deltaTime)
        {
            // get input from websockets

            if(Health < MaxHealth)
                Health += 10 * deltaTime;
                        
            base.Update(deltaTime);
        }

        public void Send(byte[] buffer)
        {
            var arraySegment = new ArraySegment<byte>(buffer);
            Socket.SendAsync(arraySegment,WebSocketMessageType.Binary,true,CancellationToken.None);
        }
    }
}