using System.Numerics;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct InputComponent
    {
        public Vector2 MovementAxis;
        public Vector2 MousePositionWorld;
        public bool Fire;

        public InputComponent(bool up, bool down, bool left, bool right, bool fire, float x, float y)
        {
            Fire = fire;

            MousePositionWorld = new Vector2(x, y);
            MovementAxis = Vector2.Zero;

            if (left)
                MovementAxis.X = -1;
            else if (right)
                MovementAxis.X = 1;

            if (up)
                MovementAxis.Y = -1;
            else if (down)
                MovementAxis.Y = 1;
        }
        public InputComponent(Vector2 moveAxis, Vector2 mousePosWorld, bool fire)
        {
            Fire = fire;

            MousePositionWorld = mousePosWorld;
            MovementAxis = moveAxis;
        }
    }
}