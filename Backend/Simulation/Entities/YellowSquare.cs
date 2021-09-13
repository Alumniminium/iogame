using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class YellowSquare : Entity
    {
        public YellowSquare(float x, float y, float vX, float vY)
        {
            Size = 30;
            Position = new Vector2(x,y);
            Velocity = new Vector2(vX,vY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Position += Velocity * deltaTime;
        }
    }
}