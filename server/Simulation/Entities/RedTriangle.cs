using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class RedTriangle : Entity
    {
        public RedTriangle(float x, float y, float vX, float vY)
        {
            Look = 3;
            Size = 300;
            Sides = 3;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
            FillColor = Convert.ToUInt32("ff5050", 16);
            BorderColor = Convert.ToUInt32("ff9999", 16);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}