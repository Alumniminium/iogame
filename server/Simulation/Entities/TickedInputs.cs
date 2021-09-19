namespace iogame.Simulation.Entities
{
    public class TickedInput
    {
        public uint Ticks;
        public bool Up, Left, Right, Down;

        public TickedInput(uint ticks, bool up, bool down, bool left, bool right)
        {
            Ticks = ticks;
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
}
