using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class YellowSquare : Entity
    {
        public YellowSquare(float x, float y, float vX, float vY)
        {
            Look = 4;
            Size = 200;
            Sides = 4;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
            FillColor = Convert.ToUInt32("ffe869", 16);
            BorderColor = Convert.ToUInt32("bfae4e", 16);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}