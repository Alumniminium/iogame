using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ShapeComponent
    {
        public byte Sides = 32;
        public ushort SizeLastFrame;
        public ushort Size;
        public float Radius => Size / 2;
        public uint Color;

        public ShapeComponent(int sides, int size, uint color)
        {
            Sides = (byte)sides;
            Size = (ushort)size;
            Color = color;
            SizeLastFrame = (ushort)size;
        }
    }
}