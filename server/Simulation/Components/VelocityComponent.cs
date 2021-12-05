using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct VelocityComponent
    {
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 Acceleration = Vector2.Zero;
    }
}