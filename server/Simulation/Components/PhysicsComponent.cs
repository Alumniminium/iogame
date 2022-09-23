using System;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Database;

namespace server.Simulation.Components
{
    [Component]
    public struct PhysicsComponent
    {
        public ShapeType ShapeType;
        public readonly Vector2 Forward => RotationRadians.AsVectorFromRadians();
        public float Radius => Size / 2;
        public readonly float InvMass => 1f / Mass;
        public float SizeLastFrame;
        public float Size;
        public float Width;
        public float Height;
        public uint Color;
        public float Density;
        public float Area => ShapeType == ShapeType.Circle ? Radius * Radius * MathF.PI : Width * Height;
        public float Mass => Area * Density;

        public float Elasticity;
        public float Drag;
        public float RotationRadians;
        public float AngularVelocity;
        public Vector2 LastPosition;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 LinearVelocity;
        private Vector2 transform;
        private readonly Vector2[] transformedVertices;
        private readonly Vector2[] vertices;
        private readonly int[] Triangles;
        public float LastRotation;
        public uint ChangedTick;
        public bool TransformUpdateRequired;

        private PhysicsComponent(Vector2 position, float restitution, float radius, float width, float height, float density, ShapeType shapeType, uint color)
        {
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
                vertices = CreateBoxVertices(Width, Height);
                Triangles = CreateBoxTriangles();
                transformedVertices = new Vector2[vertices.Length];
            }
            else
            {
                vertices = null;
                Triangles = null;
                transformedVertices = null;
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

        private static int[] CreateBoxTriangles()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
        }

        internal Vector2[] GetTransformedVertices()
        {
            if (TransformUpdateRequired)
            {
                Transform transform = new(Position, RotationRadians);

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2 v = vertices[i];
                    transformedVertices[i] = new((transform.Cos * v.X) - (transform.Sin * v.Y) + transform.PositionX, (transform.Sin * v.X) + (transform.Cos * v.Y) + transform.PositionY);
                }
                TransformUpdateRequired = false;
            }
            return transformedVertices;
        }
        public static PhysicsComponent CreateCircleBody(float radius, Vector2 position, float density, float restitution, uint color)
        {
            restitution = Math.Clamp(restitution, 0f, 1f);
            return new PhysicsComponent(position, restitution, radius, radius, radius,density, ShapeType.Circle, color);
        }

        public static PhysicsComponent CreateBoxBody(int width, int height, Vector2 position, float density, float restitution, uint color)
        {
            restitution = Math.Clamp(restitution, 0f, 1f);
            return new PhysicsComponent(position, restitution, 0f, width, height,density, ShapeType.Box, color);
        }
    }
}