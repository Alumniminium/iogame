using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class RedTriangle : Entity
    {
        public RedTriangle(float x, float y, float vX, float vY)
        {
            Look = 3;
            Size = 30;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}