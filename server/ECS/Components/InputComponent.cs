using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct InputComponent
    {
        public bool Up,Down,Left,Right,Fire;
        public float X,Y;

        public InputComponent(bool up, bool down, bool left, bool right, bool fire, float x,float y)
        {
            Up=up;
            Down = down;
            Left = left;
            Right = right;
            Fire = fire;
            X=x;
            Y=y;
        }
    }
}