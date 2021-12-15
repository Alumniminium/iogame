using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct ShapeComponent
    {
        public readonly byte Sides = 32;
        public readonly ushort Size;
        public readonly float Radius => Size / 2;
        public readonly uint Color;

        public ShapeComponent(int sides, int size, uint color)
        {
            Sides = (byte)sides;
            Size = (ushort)size;
            Color = color;
        }
    }
}