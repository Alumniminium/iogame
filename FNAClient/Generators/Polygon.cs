using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RG351MP.Helpers
{
    public partial class PolygonHelper
    {
        public class Polygon
        {
            public bool Initialized;
            public VertexBuffer Buffer;
            public Color Color;
            public VertexPositionColor[] vertexPositionColors;
            public Vector2[] Vertices;

            public Polygon(VertexPositionColor[] vertexPositionColors)
            {
                this.vertexPositionColors = vertexPositionColors;
                Vertices = new Vector2[vertexPositionColors.Length];
                for (int i = 0; i < vertexPositionColors.Length; i++)
                    Vertices[i] = new Vector2(vertexPositionColors[i].Position.X, vertexPositionColors[i].Position.Y);
            }
        }
    }
}