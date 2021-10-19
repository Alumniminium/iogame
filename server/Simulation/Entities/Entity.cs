using System.Net.Sockets;
using System.Numerics;
using iogame.Net.Packets;
using Microsoft.AspNetCore.Mvc;

namespace iogame.Simulation.Entities
{
    public class Entity
    {
        public const int VIEW_DISTANCE = 4000;
        public uint UniqueId;
        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 Velocity;
        public float Direction;
        public ushort Size;
        public float Radius => Size / 2;
        public float Mass => (float)Math.Pow(Size, 3);
        public float InverseMass => 1f / Mass;

        public float Health
        {
            get { return _health; }
            set
            {
                _health = value;
                Viewport.Send(StatusPacket.Create(UniqueId, (uint)value, StatusType.Health), true);
            }
        }

        public uint MaxSpeed;
        private float _health;
        public int MaxHealth;
        public float BodyDamage;
        public float Elasticity;
        public float Drag = Game.DRAG;
        public uint FillColor = 0;
        public uint BorderColor = 0;
        public byte Sides = 32;
        public Screen Viewport;

        public Entity()
        {
            Viewport = new(this);
            Direction = Game.Random.Next(0, 360);
            MaxSpeed = 5000;
            MaxHealth = 100;
            BodyDamage = 10;
            Health = MaxHealth;
            Elasticity = 1;
        }


        public virtual void Update(float deltaTime)
        {
            HealthRegeneration(deltaTime);
            Move(deltaTime);
            Rotate(deltaTime);
        }
        private void HealthRegeneration(float deltaTime)
        {
            if (Health < MaxHealth)
            {
                var healthAdd = 1 * deltaTime;
                if (Health + healthAdd > MaxHealth)
                    Health = MaxHealth;
                else
                    Health += healthAdd;
            }
        }
        private void Move(float deltaTime)
        {
            Velocity = Velocity.ClampMagnitude(MaxSpeed);

            Velocity *= 1 - (Drag * deltaTime);

            if (Velocity.Magnitude() < 5)
                Velocity = Vector2.Zero;

            LastPosition = Position;
            Position += Velocity * deltaTime;
            Position = Vector2.Clamp(Position, new Vector2(Radius, Radius), new Vector2(Game.MAP_WIDTH - Radius, Game.MAP_HEIGHT - Radius));
        }

        private void Rotate(float deltaTime)
        {
            var radians = Math.Atan2(Velocity.X, Velocity.Y);
            Direction = (float)(180 * radians / Math.PI);

            Direction += 0.003f * (Velocity.Y + Velocity.X) * deltaTime;

            if (Direction > 360)
                Direction = 0;
            if (Direction < 0)
                Direction = 360;
        }

        public void GetHitBy(Entity other)
        {
            Health--;
        }

        internal bool CheckCollision(Entity b) => Radius + b.Radius >= (b.Position - Position).Magnitude();
        public bool CanSee(Entity entity) => Vector2.Distance(Position, entity.Position) < VIEW_DISTANCE;
    }
}