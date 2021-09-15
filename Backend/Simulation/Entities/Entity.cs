using System.Diagnostics;
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
        public float Size;
        public int Speed;
        public float Health;
        public int MaxHealth;
        public bool InCollision;

        public Vector2 Origin => new(Position.X + Size / 2, Position.Y + Size / 2);


        public virtual void Update(float deltaTime)
        {
            if (float.IsNaN(Velocity.X) || float.IsNaN(Velocity.Y))
                Velocity.X = 10;
            if (float.IsInfinity(Velocity.X) || float.IsInfinity(Velocity.Y))
                Velocity.Y = 10;
                
            if (!InCollision && this is not Player)
            {
                if (Math.Abs(Velocity.X) > 0.05f)
                {
                    Velocity.X *= 0.9999f;
                }
                if (Math.Abs(Velocity.Y) > 0.05f)
                {
                    Velocity.Y *= 0.9999f;
                }
                if (Math.Abs(Velocity.X) < 0.05f)
                {
                    Velocity.X = 100f;
                }
                if (Math.Abs(Velocity.Y) < 0.05f)
                {
                    Velocity.Y = 100f;
                }
            }

            var radians = Math.Atan2(Velocity.X, Velocity.Y);
            Direction = (float)(180 * radians / Math.PI);

            Position += Velocity * deltaTime;
        }

        internal bool CheckCollision(Entity b)
        {
            var distance = Vector2.Distance(Origin,b.Origin);
            return distance <= Size/2 + b.Size /2;
        }
        
    }
}