using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class PurplePentagon : Entity
    {
        public PurplePentagon(float x, float y, float vX, float vY)
        {
            Size = 300;
            Sides = 5;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
            FillColor = Convert.ToUInt32("4B0082", 16);
            BorderColor = Convert.ToUInt32("9370DB", 16);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}