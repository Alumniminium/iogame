using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class PurpleOctagon : Entity
    {
        public PurpleOctagon(float x, float y, float vX, float vY)
        {
            Look = 8;
            Size = 80;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Position += Velocity * deltaTime;
        }
    }
}