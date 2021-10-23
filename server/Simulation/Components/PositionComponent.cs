using System.Numerics;
namespace iogame.Simulation.Components
{
    public class PositionComponent : GameComponent
    {
        public Vector2 Position;
        public Vector2 LastPosition;
        public float Rotation;

        public PositionComponent(float x, float y) => Position = new Vector2(x, y);
        public PositionComponent(Vector2 pos) => Position = pos;
        public void Deconstruct(out Vector2 pos, out Vector2 lastPos,out float rotation)
        {
            pos = Position;
            lastPos = LastPosition;
            rotation = Rotation;
        }
    }
}