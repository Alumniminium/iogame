using System.Numerics;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct VelocityComponent
    {
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 Acceleration = Vector2.Zero;

        public VelocityComponent(float x, float y)
        {
            Velocity = new Vector2(x, y);
        }
    }
}