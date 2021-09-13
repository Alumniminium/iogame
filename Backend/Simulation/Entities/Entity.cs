using System.Diagnostics;
using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class Entity
    {
        public uint UniqueId;
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
        }

        internal bool CheckCollision(Entity b)
        {
            var distX = Origin.X - b.Origin.X;
            var distY = Origin.Y - b.Origin.Y;
            var distance = Math.Sqrt(distX * distX + distY * distY);
            return distance < Size / 2 + b.Size / 2;
        }
    }
}