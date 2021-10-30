using System.Diagnostics.Tracing;
using System.Net.WebSockets;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Util;

namespace iogame.Simulation.Entities
{
    public class Player : Entity
    {
        public string Name = "Unnamed";
        public string Password = "";

        public bool Up, Left, Right, Down, Fire;
        public float FireDir;
        public TickedInput[] TickedInputs = new TickedInput[5];
        public Dictionary<uint, Vector2> LastEntityPositions = new();
        public WebSocket Socket;
        public byte[] RecvBuffer;
        public uint LastShot;

        public Player(WebSocket socket)
        {
            VelocityComponent = new VelocityComponent(0, 0, maxSpeed: 1500);
            ShapeComponent = new ShapeComponent(sides: 32, size: 200);
            HealthComponent = new HealthComponent(1000, 1000, 0);
            var mass = (float)Math.Pow(ShapeComponent.Size, 3);
            PhysicsComponent = new PhysicsComponent(mass, 1, 0.999f);

            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }

        public void Attack()
        {
            if (LastShot + 5 <= Game.CurrentTick)
            {
                LastShot = Game.CurrentTick;
                var speed = 1000;
                var dx = (float)Math.Cos(FireDir);
                var dy = (float)Math.Sin(FireDir);

                var bulletX = -dx + PositionComponent.Position.X;
                var bulletY = -dy + PositionComponent.Position.Y;
                var bullet = SpawnManager.Spawn<Bullet>(new Vector2(bulletX, bulletY));
                bullet.LifeTimeSeconds = 5;

                var dist = PositionComponent.Position - bullet.PositionComponent.Position;
                var pen_depth = ShapeComponent.Radius + bullet.ShapeComponent.Radius - dist.Magnitude();
                var pen_res = dist.Unit() * pen_depth * 1.125f;

                bullet.PositionComponent.Position += pen_res;
                bullet.VelocityComponent.Movement = new Vector2(dx * speed, dy * speed);
                bullet.HealthComponent.Health = 100;
                bullet.HealthComponent.MaxHealth = 100;
                bullet.BodyDamage = 10;

                bullet.SetOwner(this);

                Viewport.Add(bullet, true);
            }
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

        private void Disconnect()
        {
            OutgoingPacketQueue.Remove(this);
            EntityManager.RemoveEntity(this);
        }
    }
}