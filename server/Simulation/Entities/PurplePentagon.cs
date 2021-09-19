using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class PurplePentagon : Entity
    {
        public PurplePentagon(float x, float y, float vX, float vY)
        {
            Look = 5;
            Size = 40;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}