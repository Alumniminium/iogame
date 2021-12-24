using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct PositionComponent
    {
        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 LastSyncedPosition;
        public float Rotation;

        public PositionComponent(Vector2 position)
        {
            LastSyncedPosition = position;
            Position = position;
            LastPosition=position;
            Rotation = 0f;
        }
    }
}