using System.Numerics;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct VelocityComponent
    {
        public Vector2 Force = Vector2.Zero;
        public float Spin = 0;

        public VelocityComponent(float x, float y)
        {
            Force = new Vector2(x, y);
        }
    }
}