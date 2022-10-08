using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Packets.Enums;

namespace RG351MP.Helpers
{
    internal readonly struct Transform
    {
        public readonly float PositionX;
        public readonly float PositionY;
        public readonly float Sin;
        public readonly float Cos;

        public static readonly Transform Zero = new(0f, 0f, 0f);

        public Transform(Vector2 position, float angle)
        {
            PositionX = position.X;
            PositionY = position.Y;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }

        public Transform(float x, float y, float angle)
        {
            PositionX = x;
            PositionY = y;
            Sin = MathF.Sin(angle);
            Cos = MathF.Cos(angle);
        }
    }
    public partial class PolygonHelper
    {
        public static VertexPositionColor[] GenerateShape(ShapeType shapeType, float width, float height, Color color, float rotation)
        {
            var vectors = new List<Vector2>();
            if (shapeType == ShapeType.Circle)
            {
                var step = 2 * MathF.PI / 32;
                for (var i = 0; i <= 32; i++)
                {
                    var curStep = i * step;
                    vectors.Add(new Vector2(width/2 * MathF.Cos(curStep), width/2 * MathF.Sin(curStep)));
                }
            }
            else if (shapeType == ShapeType.Triangle)
            {
                vectors.Add(new Vector2(width/2, -height/2));
                vectors.Add(new Vector2(0, height/2));
                vectors.Add(new Vector2(-width/2, -height/2));
            }
            else if (shapeType == ShapeType.Box)
            {
                float left = -width / 2f;
                float right = left + width;
                float bottom = -height / 2f;
                float top = bottom + height;

                vectors.Add(new Vector2(left, top));
                vectors.Add(new Vector2(right, top));
                vectors.Add(new Vector2(right, bottom));
                vectors.Add(new Vector2(left, bottom));
            }

            return TriangulateConvexPolygon(vectors, color);
    }


    public static VertexPositionColor[] TriangulateConvexPolygon(List<Vector2> convexHullpoints, Color color)
    {
        List<VertexPositionColor> triangles = new();

        for (int i = 2; i < convexHullpoints.Count; i++)
        {
            VertexPositionColor a = new(new Vector3(convexHullpoints[0], 0), color);
            VertexPositionColor b = new(new Vector3(convexHullpoints[i - 1], 0), color);
            VertexPositionColor c = new(new Vector3(convexHullpoints[i], 0), color);

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        return triangles.ToArray();
    }

}
}