using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct PositionComponent
    {
        public Vector2 Position;
        public Vector2 LastPosition;
        public float Rotation;
    }
}