using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct ShapeComponent
    {
        public byte Sides = 32;
        public ushort Size;
        public float Radius => Size/2;

        public ShapeComponent(byte sides, ushort size)
        {
            Sides = sides;
            Size = size;
        }
    }
}