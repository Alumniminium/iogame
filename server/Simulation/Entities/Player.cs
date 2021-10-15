using System.Net.WebSockets;
using System.Numerics;
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
            Size = 200;
            MaxSpeed = 1500;
            Drag = 0.999f;
            Socket = socket;
            RecvBuffer = new byte[1024 * 4];
        }

        public override void Update(float deltaTime)
        {
            ProcessInputs(deltaTime);

            HealthRegeneration(deltaTime);

            base.Update(deltaTime);
        }

        private void ProcessInputs(float deltaTime)
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
            inputVector *= deltaTime;

            Velocity += inputVector;

            if (Fire)
                Attack();
        }

        private void Attack()
        {
            if (LastShot + 3 <= Game.CurrentTick)
            {
                LastShot = Game.CurrentTick;
                var speed = 1000;
                var dx = (float)Math.Cos(FireDir);
                var dy = (float)Math.Sin(FireDir);

                var id = IdGenerator.Get<Bullet>();

                var bullet = new Bullet(id, this)
                {
                    Position = new Vector2(-dx + Position.X, -dy + Position.Y),
                    Velocity = new Vector2(dx, dy) * speed,
                    Direction = 0,
                    SpawnTime = Game.CurrentTick,
                    Drag = 0,
                    Elasticity = 0
                };

                var dist = Position - bullet.Position;
                var pen_depth = Radius + bullet.Radius - dist.Magnitude();
                var pen_res = dist.Unit() * pen_depth * 1.1f;

                bullet.Position += pen_res;
                Game.AddEntity(bullet);
            }
        }

        private void HealthRegeneration(float deltaTime)
        {
            if (Health < MaxHealth)
            {
                var healthAdd = 10 * deltaTime;
                if (Health + healthAdd > MaxHealth)
                    Health = MaxHealth;
                else
                    Health += healthAdd;
            }
        }

        internal void AddMovement(uint ticks, bool up, bool down, bool left, bool right)
        {
            var idx = ticks % 5;
            var tickedInput = new TickedInput(ticks, up, down, left, right);
            TickedInputs[idx] = tickedInput;
        }

        public void Send(byte[] buffer) => Game.OutgoingPacketBuffer.Add(this, buffer);
        public async Task ForceSendAsync(byte[] buffer, int count)
        {
            try
            {
                var arraySegment = new ArraySegment<byte>(buffer,0,count);
                await Socket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch
            {
                Game.OutgoingPacketBuffer.Packets[this].Clear();
            }
        }
    }
}