using System.Numerics;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct VelocityComponent
    {
        public Vector2 Movement = Vector2.Zero;
        public float Spin = 0;

        public VelocityComponent(float x, float y)
        {
            Movement = new Vector2(x, y);
        }
    }
}