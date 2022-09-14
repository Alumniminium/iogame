using server.ECS;

namespace server.Simulation.Components
{
    public enum ShapeType
    {
        Sphere,
        Triangle,
        Rectangle,
    }

    [Component]
    public struct ShapeComponent
    {
        public ShapeType Type; 
        public byte Sides = 32;
        public ushort SizeLastFrame;
        public ushort Size;
        public float Radius => Size / 2;
        public uint Color;

        public ShapeComponent(int sides, int size, uint color)
        {
            if(sides == 3)
                Type = ShapeType.Triangle;
            else if(sides == 4)
                Type = ShapeType.Rectangle;
            else
                Type = ShapeType.Sphere;

            Sides = (byte)sides;
            Size = (ushort)size;
            Color = color;
            SizeLastFrame = (ushort)size;
        }
    }
}