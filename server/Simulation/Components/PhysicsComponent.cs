using System;
using System.Numerics;
using Packets.Enums;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    [Component]
    public struct PhysicsComponent
    {
        public readonly int EntityId;
        public readonly ShapeType ShapeType;
        public readonly Vector2 Forward => RotationRadians.AsVectorFromRadians();
        public readonly float Radius => Size / 2;
        public readonly float InvMass => 1f / Mass;
        public readonly float InvInertia => Inertia > 0f ? 1f / Inertia : 0f;
        public readonly float Area => ShapeType == ShapeType.Circle ? Radius * Radius * MathF.PI : Width * Height;
        public readonly float Mass => Area * Density;
        public readonly Memory<Vector2> transformedVertices;
        public readonly Memory<Vector2> Vertices;
        public readonly int Sides;
        public readonly uint Color;
        public readonly float Density;
        public readonly float Elasticity;
        public float Inertia;
        public float Drag;
        public float SizeLastFrame;
        public float Size;
        public float Width;
        public float Height;
        public float RotationRadians;
        public float AngularVelocity;
        public Vector2 LastPosition;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 LinearVelocity;
        // private Vector2 transform;
        public float LastRotation;
        public uint ChangedTick;
        public bool TransformUpdateRequired;

        private PhysicsComponent(int entityId, Vector2 position, float restitution, float radius, float width, float height, float density, ShapeType shapeType, uint color, int sides = 4)
        {
            EntityId = entityId;
            Sides = sides;
            Position = position;
            LinearVelocity = Vector2.Zero;
            RotationRadians = 0f;
            AngularVelocity = 0f;

            Acceleration = Vector2.Zero;

            Density = density;
            Elasticity = restitution;

            Size = radius * 2;
            Width = width;
            Height = height;
            ShapeType = shapeType;

            if (ShapeType == ShapeType.Box)
            {
                if (Sides == 4)
                    Vertices = CreateBoxVertices(Width, Height);
                else if (Sides == 3)
                    Vertices = CreateTriangleVertices(Width, Height);

                // Triangles = CreateBoxTriangles();
                transformedVertices = new Vector2[Vertices.Length];
                Inertia = 1f / 12f * Mass * (Width * Width + Height * Height);
            }
            else
            {
                Vertices = null;
                transformedVertices = null;
                Inertia = 1f / 2f * Mass * Radius * Radius;
            }
            Drag = 0.01f;
            Color = color;
            TransformUpdateRequired = true;
            ChangedTick = Game.CurrentTick;
        }
        private static Vector2[] CreateBoxVertices(float width, float height)
        {
            float left = -width / 2f;
            float right = left + width;
            float bottom = -height / 2f;
            float top = bottom + height;

            Vector2[] vertices = new Vector2[4];
            vertices[0] = new Vector2(left, top);
            vertices[1] = new Vector2(right, top);
            vertices[2] = new Vector2(right, bottom);
            vertices[3] = new Vector2(left, bottom);

            return vertices;
        }
        // private static Vector2[] CreateTriangleVertices(float width, float height)
        // {
        //     float left = -width / 2f;
        //     float right = left + width;
        //     float bottom = -height / 2f;
        //     float top = bottom + height;

        //     Vector2[] vertices = new Vector2[3];
        //     vertices[0] = new Vector2(left, top);
        //     vertices[1] = new Vector2(right, top);
        //     vertices[2] = new Vector2(left, right);

        //     return vertices;
        // }
        private static Vector2[] CreateTriangleVertices(float c, float b)
        {
            Vector2[] vertices = new Vector2[3];
            var a = c / 2;

            vertices[0] = new Vector2(-a, -b / 2);
            vertices[1] = new Vector2(a, -b / 2);
            vertices[2] = new Vector2(0, b / 2);

            return vertices;
        }

        // private static int[] CreateBoxTriangles()
        // {
        //     int[] triangles = new int[6];
        //     triangles[0] = 0;
        //     triangles[1] = 1;
        //     triangles[2] = 2;
        //     triangles[3] = 0;
        //     triangles[4] = 2;
        //     triangles[5] = 3;
        //     return triangles;
        // }
        // private static int[] CreateTriangleTriangles()
        // {
        //     int[] triangles = new int[3];
        //     triangles[0] = 0;
        //     triangles[1] = 1;
        //     triangles[2] = 2;
        //     return triangles;
        // }

        internal Memory<Vector2> GetTransformedVertices()
        {
            if (TransformUpdateRequired)
            {
                Transform transform = new(Position, RotationRadians);

                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vector2 v = Vertices.Span[i];
                    transformedVertices.Span[i] = new((transform.Cos * v.X) - (transform.Sin * v.Y) + transform.PositionX, (transform.Sin * v.X) + (transform.Cos * v.Y) + transform.PositionY);
                }
                TransformUpdateRequired = false;
            }
            return transformedVertices;
        }
        public static PhysicsComponent CreateCircleBody(int entityId, float radius, Vector2 position, float density, float restitution, uint color)
        {
            restitution = Math.Clamp(restitution, 0f, 1f);
            return new PhysicsComponent(entityId, position, restitution, radius, radius, radius, density, ShapeType.Circle, color, 0);
        }

        public static PhysicsComponent CreateBoxBody(int entityId, int width, int height, Vector2 position, float density, float restitution, uint color)
        {
            restitution = Math.Clamp(restitution, 0f, 1f);
            return new PhysicsComponent(entityId, position, restitution, 0f, width, height, density, ShapeType.Box, color, 4);
        }
        public static PhysicsComponent CreateTriangleBody(int entityId, int width, int height, Vector2 position, float density, float restitution, uint color)
        {
            restitution = Math.Clamp(restitution, 0f, 1f);
            return new PhysicsComponent(entityId, position, restitution, 0f, width, height, density, ShapeType.Box, color, 3);
        }
        public override int GetHashCode() => EntityId;
    }
}