namespace iogame.Simulation.Components
{
    public class ShapeComponent : GameComponent
    {
        public byte Sides = 32;
        public ushort Size;
        public float Radius;

        public ShapeComponent(byte sides, ushort size)
        {
            Sides = sides;
            Size = size;
            Radius = Size/2;
        }
    }
}