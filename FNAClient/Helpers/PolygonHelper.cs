using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RG351MP.Helpers
{
    public partial class PolygonHelper
    {
        static readonly Dictionary<int, VertexPositionColor[]> ShapeCache = new();
        public static VertexPositionColor[] GenerateShape(int sides, float radius, Color color)
        {
            if (!ShapeCache.TryGetValue(sides, out var vertexPositionColors))
            {
                var vectors = new List<Vector2>();
                var step = 2 * MathF.PI / sides;
                for (var i = 0; i <= sides; i++)
                {
                    var curStep = i * step;
                    vectors.Add(new Vector2(radius * MathF.Cos(curStep), radius * MathF.Sin(curStep)));
                }
                vertexPositionColors = TriangulateConvexPolygon(vectors, color);
                ShapeCache.Add(sides, vertexPositionColors);
            }
            return vertexPositionColors;
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