using System.Numerics;
 
namespace iogame.Simulation.Entities
{
    public class Entity
    {
        public uint UniqueId;
        public uint Look;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Direction;
        public ushort Size;
        public float Radius => Size / 2;
        public ushort Mass => (ushort)Math.Floor(Math.Pow(Size,3));
        public int Speed;
        public float Health;
        public int MaxHealth;
        public bool InCollision;
        public float Drag = Game.DRAG;

        public Vector2 Origin => new(Position.X + Size / 2, Position.Y + Size / 2);

        public Entity()
        {
            Direction = Game.random.Next(0,360);
            Speed = 5000;
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

            if(!InCollision)
            Velocity *=1 -(Drag * deltaTime);

            Velocity = Velocity.ClampMagnitude(Speed);
            //Velocity *= deltaTime;
            
            Position += Velocity * deltaTime;
        }

        internal bool CheckCollision(Entity b)
        {
            var distance = Vector2.Distance(Origin, b.Origin);
            return distance <= Radius + b.Radius;
        }

    }
}