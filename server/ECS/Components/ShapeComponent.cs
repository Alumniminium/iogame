using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct ShapeComponent
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