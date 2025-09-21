using System;
using System.Collections.Concurrent;
using System.Numerics;
using Box2D.NET;
using server.Enums;
using server.ECS;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Geometries;
using static Box2D.NET.B2Hulls;

namespace server.Simulation.Components;

public static class Box2DPhysicsWorld
{
    public static B2WorldId WorldId { get; private set; }
    private static readonly ConcurrentQueue<Action> _actionQueue = new();

    static Box2DPhysicsWorld()
    {
        Initialize();
    }

    private static void Initialize()
    {
        var worldDef = b2DefaultWorldDef();
        worldDef.gravity = new B2Vec2(0, 9.81f); // Y-down gravity
        worldDef.enableSleep = true;
        worldDef.workerCount = 16; // Multi-threaded Box2D
        WorldId = b2CreateWorld(ref worldDef);
    }

    public static void Step(float deltaTime, int subStepCount = 4)
    {
        // Process queued actions before stepping
        while (_actionQueue.TryDequeue(out var action))
            action();

        b2World_Step(WorldId, deltaTime, subStepCount);
    }

    public static void QueueAction(Action action)
    {
        _actionQueue.Enqueue(action);
    }

    public static B2BodyId CreateBody(Vector2 position, float rotation, bool isStatic, ShapeType shapeType, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = isStatic ? B2BodyType.b2_staticBody : B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);
        bodyDef.rotation = new B2Rot(MathF.Cos(rotation), MathF.Sin(rotation));
        bodyDef.enableSleep = true;
        bodyDef.angularDamping = 0.01f; // Very light angular damping
        bodyDef.linearDamping = 0.1f;   // Keep linear damping

        var bodyId = b2CreateBody(WorldId, ref bodyDef);

        // Create shape based on type
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;

        switch (shapeType)
        {
            case ShapeType.Box:
                var box = b2MakeBox(0.5f, 0.5f); // 1x1 box
                b2CreatePolygonShape(bodyId, ref shapeDef, ref box);
                break;

            case ShapeType.Circle:
                var circle = new B2Circle(new B2Vec2(0, 0), 0.5f); // radius 0.5 for 1x1 circle
                b2CreateCircleShape(bodyId, ref shapeDef, ref circle);
                break;

            case ShapeType.Triangle:
                // Create 1x1 triangle vertices
                var trianglePoints = new B2Vec2[3]
                {
                    new B2Vec2(0, -0.5f),    // Top
                    new B2Vec2(-0.5f, 0.5f), // Bottom left
                    new B2Vec2(0.5f, 0.5f)   // Bottom right
                };
                var triangleHull = b2ComputeHull(trianglePoints.AsSpan(), 3);
                var triangle = b2MakePolygon(ref triangleHull, 0f);
                b2CreatePolygonShape(bodyId, ref shapeDef, ref triangle);
                break;

            default:
                // Default to 1x1 box
                var defaultBox = b2MakeBox(0.5f, 0.5f);
                b2CreatePolygonShape(bodyId, ref shapeDef, ref defaultBox);
                break;
        }

        return bodyId;
    }

    public static B2BodyId CreateCircleBody(Vector2 position, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
    {
        return CreateBody(position, 0f, isStatic, ShapeType.Circle, density, friction, restitution);
    }

    public static B2BodyId CreateBoxBody(Vector2 position, float rotation, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
    {
        return CreateBody(position, rotation, isStatic, ShapeType.Box, density, friction, restitution);
    }

    public static void DestroyBody(B2BodyId bodyId)
    {
        if (b2Body_IsValid(bodyId))
        {
            QueueAction(() => b2DestroyBody(bodyId));
        }
    }

    public static void CreateMapBorders(Vector2 mapSize)
    {
        // Create static body for all border edges
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_staticBody;
        bodyDef.position = new B2Vec2(0, 0);
        var borderBody = b2CreateBody(WorldId, ref bodyDef);

        var shapeDef = b2DefaultShapeDef();
        shapeDef.material.friction = 0.3f;
        shapeDef.material.restitution = 0.2f;

        // Top wall (y = 0)
        var topEdge = new B2Segment(new B2Vec2(0, 0), new B2Vec2(mapSize.X, 0));
        b2CreateSegmentShape(borderBody, ref shapeDef, ref topEdge);

        // Bottom wall (y = mapHeight)
        var bottomEdge = new B2Segment(new B2Vec2(0, mapSize.Y), new B2Vec2(mapSize.X, mapSize.Y));
        b2CreateSegmentShape(borderBody, ref shapeDef, ref bottomEdge);

        // Left wall (x = 0)
        var leftEdge = new B2Segment(new B2Vec2(0, 0), new B2Vec2(0, mapSize.Y));
        b2CreateSegmentShape(borderBody, ref shapeDef, ref leftEdge);

        // Right wall (x = mapWidth)
        var rightEdge = new B2Segment(new B2Vec2(mapSize.X, 0), new B2Vec2(mapSize.X, mapSize.Y));
        b2CreateSegmentShape(borderBody, ref shapeDef, ref rightEdge);

        Console.WriteLine($"Created map borders: {mapSize.X}x{mapSize.Y}");
    }

    public static void Shutdown()
    {
        b2DestroyWorld(WorldId);
        WorldId = default;
    }
}