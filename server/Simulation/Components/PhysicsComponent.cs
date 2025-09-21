using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;

namespace server.Simulation.Components;

[Component]
public struct PhysicsComponent
{
    public readonly ShapeType ShapeType;
    public readonly Vector2 Forward => RotationRadians.AsVectorFromRadians();
    public readonly float Radius => Size / 2;
    public readonly float InvMass => 1f / Mass;
    public readonly float InvInertia => Inertia > 0f ? 1f / Inertia : 0f;
    public readonly float Area => ShapeType == ShapeType.Circle ? Radius * Radius * MathF.PI : Width * Height;
    public readonly float Mass => Area * Density;

    public bool Static;

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
    public float LastRotation;
    public long ChangedTick;
    public bool TransformUpdateRequired;
    public bool AABBUpdateRequired;

    private PhysicsComponent(Vector2 position, float restitution, float radius, float width, float height, float density, ShapeType shapeType, uint color, int sides = 4, bool _static = false)
    {
        Static = _static;
        Sides = sides;
        Position = position;
        LastPosition = position;
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

        if (ShapeType == ShapeType.Box || ShapeType == ShapeType.Triangle)
        {
            if (Sides == 4)
                Vertices = CreateBoxVertices(Width, Height);
            else if (Sides == 3)
                Vertices = CreateTriangleVertices(Width, Height);

            transformedVertices = new Vector2[Vertices.Length];
            Inertia = 1f / 12f * Mass * (Width * Width + Height * Height);
        }
        else
        {
            Vertices = null;
            transformedVertices = null;
            Inertia = 1f / 2f * Mass * Radius * Radius;
        }
        Drag = 0.02f;
        Color = color;
        TransformUpdateRequired = true;
        AABBUpdateRequired = true;
        ChangedTick = NttWorld.Tick;
    }
    private static Vector2[] CreateBoxVertices(float width, float height)
    {
        float left = -width / 2f;
        float right = left + width;
        float bottom = -height / 2f;
        float top = bottom + height;

        Vector2[] vertices =
        [
            new Vector2(left, top),
            new Vector2(right, top),
            new Vector2(right, bottom),
            new Vector2(left, bottom),
        ];
        return vertices;
    }
    private static Vector2[] CreateTriangleVertices(float c, float b)
    {
        Vector2[] vertices = [new Vector2(-c / 2, -b / 2), new Vector2(c / 2, -b / 2), new Vector2(0, b / 2)];
        return vertices;
    }

    internal Memory<Vector2> GetTransformedVertices()
    {
        if (TransformUpdateRequired)
        {
            for (int i = 0; i < Vertices.Length; i++)
                transformedVertices.Span[i] = Vector2.Transform(Vertices.Span[i], Matrix4x4.CreateRotationZ(RotationRadians) * Matrix4x4.CreateTranslation(Position.X, Position.Y, 0));
            TransformUpdateRequired = false;
        }
        return transformedVertices;
    }
    public static PhysicsComponent CreateCircleBody(float radius, Vector2 position, float density, float restitution, uint color, bool _static = false)
    {
        restitution = Math.Clamp(restitution, 0f, 1f);
        return new PhysicsComponent(position, restitution, radius, radius, radius, density, ShapeType.Circle, color, 0, _static);
    }

    public static PhysicsComponent CreateBoxBody(int width, int height, Vector2 position, float density, float restitution, uint color, bool _static = false)
    {
        restitution = Math.Clamp(restitution, 0f, 1f);
        return new PhysicsComponent(position, restitution, 0f, width, height, density, ShapeType.Box, color, 4, _static);
    }
    public static PhysicsComponent CreateTriangleBody(int width, int height, Vector2 position, float density, float restitution, uint color, bool _static = false)
    {
        restitution = Math.Clamp(restitution, 0f, 1f);
        return new PhysicsComponent(position, restitution, 0f, width, height, density, ShapeType.Triangle, color, 3, _static);
    }


}