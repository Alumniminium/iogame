using System.Numerics;

namespace iogame.Simulation.Entities
{
    public class PurpleOctagon : Entity
    {
        public PurpleOctagon(float x, float y, float vX, float vY)
        {
            Size = 800;
            Position = new Vector2(x, y);
            Velocity = new Vector2(vX, vY);
        }

        public override async Task UpdateAsync(float deltaTime)
        {
            await base.UpdateAsync(deltaTime);
        }
    }
}