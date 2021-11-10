using System.Diagnostics.Tracing;
using System.Net.WebSockets;
using System.Numerics;
using iogame.Simulation.Components;
using iogame.Util;
using iogame.Simulation.Managers;

namespace iogame.Simulation.Entities
{
    public class Player : ShapeEntity
    {
        public string Name = "Unnamed";
        public string Password = "";

        public ref InputComponent InputComponent => ref Entity.Get<InputComponent>();
        public float FireDir;
        public TickedInput[] TickedInputs = new TickedInput[5];
        public Dictionary<uint, Vector2> LastEntityPositions = new();
        public WebSocket Socket;
        public byte[] RecvBuffer;
        public uint LastShot;

        public Player(WebSocket socket)
        {
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }

        public void Attack()
        {
            if (LastShot + 1 <= Game.CurrentTick)
            {
                LastShot = Game.CurrentTick;
                var speed = 1000;
                var dx = (float)Math.Cos(FireDir);
                var dy = (float)Math.Sin(FireDir);

                var bulletX = -dx + PositionComponent.Position.X;
                var bulletY = -dy + PositionComponent.Position.Y;
                var bullet = SpawnManager.Spawn<Bullet>(new Vector2(bulletX, bulletY));
                ref var pos = ref bullet.PositionComponent;
                var vel = new VelocityComponent(0, 0);
                var spd = new SpeedComponent(1000);
                var shp = new ShapeComponent(sides: 0, size: 25);
                var hlt = new HealthComponent(20,20,0);
                var phy = new PhysicsComponent((float)Math.Pow(ShapeComponent.Size, 3), 0,0);
                var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(5));

                var dist = PositionComponent.Position - pos.Position;
                var pen_depth = ShapeComponent.Radius + shp.Radius - dist.Magnitude();
                var pen_res = dist.Unit() * pen_depth * 1.125f;
                pos.Position += pen_res;
                vel.Movement = new Vector2(dx * speed, dy * speed);

                bullet.Entity.Add(vel);
                bullet.Entity.Add(shp);
                bullet.Entity.Add(hlt);
                bullet.Entity.Add(phy);
                bullet.Entity.Add(ltc);
                bullet.Entity.Add(spd);

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

        public void Disconnect()
        {
            OutgoingPacketQueue.Remove(this);
            World.Destroy(Entity.EntityId);
        }
    }
}