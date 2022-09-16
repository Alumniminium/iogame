using System;
using System.Numerics;
using FlatPhysics;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Components
{
    public enum ShapeType
    {
        Circle,
        Triangle,
        Box,
    }
    [Component]
    public struct PhysicsComponent
    {
        public ShapeType ShapeType;
        public ushort SizeLastFrame;
        public ushort Size;
        public short Width;
        public short Height;

        public float Radius => Size / 2;
        public uint Color;
        public float Mass;
        public readonly float InvMass => 1f / Mass;
        // public float InvMass;
        public float Restitution;
        public float Drag;
        public float Rotation;
        public float RotationalVelocity;
        public Vector2 LastPosition;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 LinearVelocity;
        private Vector2 transform;
        private Vector2[] transformedVertices;
        private Vector2[] vertices;
        private int[] Triangles;

        public readonly Vector2 Forward => Rotation.AsVectorFromRadians();

        public float LastRotation { get; internal set; }

        public uint ChangedTick;
        public bool TransformUpdateRequired;
        
        private PhysicsComponent(Vector2 position, float mass, float restitution, float radius, int width, int height, ShapeType shapeType, uint color)
        {
            Position = position;
            LinearVelocity = Vector2.Zero;
            Rotation = 0f;
            RotationalVelocity = 0f;

            Acceleration = Vector2.Zero;

            Mass = mass;
            Restitution = restitution;

            Size = (ushort)(radius * 2);
            Width = (short)width;
            Height = (short)height;
            ShapeType = shapeType;

            if (this.ShapeType is ShapeType.Box)
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
            Color = color;
            TransformUpdateRequired = true;
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
                FlatTransform transform = new(Position, Rotation);

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2 v = vertices[i];
                    transformedVertices[i] = new(transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX, transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
                }
            }

            TransformUpdateRequired = false;
            return transformedVertices;
        }

        public void Move(Vector2 amount)
        {
            LastPosition = Position;
            Position += amount;
            TransformUpdateRequired = true;
        }

        public void MoveTo(Vector2 position)
        {
            LastPosition = Position;
            Position = position;
            TransformUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            Rotation += amount;
            TransformUpdateRequired = true;
        }

        public void AddForce(Vector2 amount)
        {
            Acceleration = amount;
        }

        public static PhysicsComponent CreateCircleBody(float radius, Vector2 position, float density, float restitution, uint color)
        {
            float area = radius * radius * MathF.PI;

            restitution = Math.Clamp(restitution, 0f, 1f);

            // mass = area * depth * density
            float mass = area * density;

            return new PhysicsComponent(position, mass, restitution, radius, 0, 0, ShapeType.Circle, color);
        }

        public static PhysicsComponent CreateBoxBody(int width, int height, Vector2 position, float density, float restitution,uint color)
        {
            float area = width * height;

            restitution = Math.Clamp(restitution, 0f, 1f);

            // mass = area * depth * density
            float mass = area * density;

            return new PhysicsComponent(position, mass, restitution, 0f, width, height, ShapeType.Box, color);
        }
    }
}