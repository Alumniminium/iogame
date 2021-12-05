using System.Numerics;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct PositionComponent
    {
        public Vector2 Position;
        public Vector2 LastPosition;
        public float Rotation = 0;

        public PositionComponent(float x, float y)
        {
            Position = new Vector2(x, y);
            LastPosition = new Vector2(x, y);
            Rotation = 0;
        }

        public PositionComponent(Vector2 pos)
        {
            Position = pos;
            LastPosition = pos;
        }

        public void Deconstruct(out Vector2 pos, out Vector2 lastPos,out float rotation)
        {
            pos = Position;
            lastPos = LastPosition;
            rotation = Rotation;
        }
    }
}