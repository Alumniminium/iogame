using System.Numerics;
 
namespace iogame.Simulation.Entities
{
    public class Entity
    {
        public const int VIEW_DISTANCE = 3000;
        public uint UniqueId;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Direction;
        public ushort Size;
        public float Radius => Size / 2;
        public float Mass => (float)Math.Pow(Size,3);
        public float InverseMass => 1f / Mass;
        public uint MaxSpeed;
        public float Health;
        public int MaxHealth;
        public float Elasticity;
        public float Drag = Game.DRAG;
        public uint FillColor = 0;
        public uint BorderColor = 0;
        public byte Sides = 32;

        public Entity()
        {
            Direction = Game.random.Next(0,360);
            MaxSpeed = 5000;
            MaxHealth = 100;
            Health = MaxHealth;
            Elasticity=1;
        }


        public virtual void Update(float deltaTime)
        {
            if (float.IsNaN(Velocity.X) || float.IsNaN(Velocity.Y))
                Velocity.X = 0;
            if (float.IsInfinity(Velocity.X) || float.IsInfinity(Velocity.Y))
                Velocity.Y = 0;

            var radians = Math.Atan2(Velocity.X, Velocity.Y);
            Direction = (float)(180 * radians / Math.PI);

            Direction += 0.003f * (Velocity.Y + Velocity.X) * deltaTime;

            if (Direction > 360)
                Direction = 0;
            if (Direction < 0)
                Direction = 360;

            Velocity *= 1 -(Drag * deltaTime);

            Velocity = Velocity.ClampMagnitude(MaxSpeed);
            
            Position += Velocity * deltaTime;
        }

        internal bool CheckCollision(Entity b)
        {
            return Radius + b.Radius >= (b.Position - Position).Magnitude();
        }

        public bool CanSee(Entity entity) => (Vector2.Distance(Position, entity.Position) < Player.VIEW_DISTANCE);
    }
}