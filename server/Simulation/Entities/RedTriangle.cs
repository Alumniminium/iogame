using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class RedTriangle : Entity
    {
        public RedTriangle(float x, float y, float vX, float vY)
        {
            Size = 200;
            Sides = 3;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
            FillColor = Convert.ToUInt32("ff5050", 16);
            BorderColor = Convert.ToUInt32("ff9999", 16);
        }
    }
}