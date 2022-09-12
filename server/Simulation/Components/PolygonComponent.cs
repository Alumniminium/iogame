using System.Collections.Generic;
using System.Numerics;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct PolygonComponent
    {
        public readonly List<Vector2> Edges = new();
        public readonly List<Vector2> Points = new();
        public readonly Vector2 Center()
        {
            float totalX = 0;
            float totalY = 0;
            for (var i = 0; i < Points.Count; i++)
            {
                totalX += Points[i].X;
                totalY += Points[i].Y;
            }

            return new Vector2(totalX / Points.Count, totalY / Points.Count);
        }

        public readonly void Offset(Vector2 v)
        {
            Offset(v.X, v.Y);
        }

        public readonly void Offset(float x, float y)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                var p = Points[i];
                Points[i] = new Vector2(p.X + x, p.Y + y);
            }
        }


        public void BuildEdges()
        {
            Vector2 p1;
            Vector2 p2;
            Edges.Clear();
            for (var i = 0; i < Points.Count; i++)
            {
                p1 = Points[i];
                p2 = i + 1 >= Points.Count ? Points[0] : Points[i + 1];

                Edges.Add(p2 - p1);
            }
        }

        public PolygonComponent()
        {
            Edges = new();
            Points = new();

            BuildEdges();
        }
    }
}