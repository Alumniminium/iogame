using System.Drawing;
using iogame.ECS;

namespace iogame.Simulation.Components
{
    [Component]
    public struct ShapeComponent
    {
        public byte Sides = 32;
        public ushort Size;
        public float Radius => Size/2;

        public uint BorderColor;
        public uint Color;
    }
}