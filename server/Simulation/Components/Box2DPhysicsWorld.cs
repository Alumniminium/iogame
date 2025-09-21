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

    public static B2BodyId CreateBody(Vector2 position, float rotation, bool isStatic, ShapeType shapeType, float width, float height, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
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
                var box = b2MakeBox(width / 2f, height / 2f);
                b2CreatePolygonShape(bodyId, ref shapeDef, ref box);
                break;

            case ShapeType.Circle:
                var circle = new B2Circle(new B2Vec2(0, 0), width / 2f);
                b2CreateCircleShape(bodyId, ref shapeDef, ref circle);
                break;

            case ShapeType.Triangle:
                // Create triangle vertices
                var trianglePoints = new B2Vec2[3]
                {
                    new B2Vec2(0, -height / 2f),           // Top
                    new B2Vec2(-width / 2f, height / 2f),  // Bottom left
                    new B2Vec2(width / 2f, height / 2f)    // Bottom right
                };
                var triangleHull = b2ComputeHull(trianglePoints.AsSpan(), 3);
                var triangle = b2MakePolygon(ref triangleHull, 0f);
                b2CreatePolygonShape(bodyId, ref shapeDef, ref triangle);
                break;

            default:
                // Default to box
                var defaultBox = b2MakeBox(width / 2f, height / 2f);
                b2CreatePolygonShape(bodyId, ref shapeDef, ref defaultBox);
                break;
        }

        return bodyId;
    }

    public static B2BodyId CreateCircleBody(Vector2 position, float radius, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
    {
        return CreateBody(position, 0f, isStatic, ShapeType.Circle, radius * 2f, radius * 2f, density, friction, restitution);
    }

    public static B2BodyId CreateBoxBody(Vector2 position, float rotation, float width, float height, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f)
    {
        return CreateBody(position, rotation, isStatic, ShapeType.Box, width, height, density, friction, restitution);
    }

    public static void DestroyBody(B2BodyId bodyId)
    {
        if (b2Body_IsValid(bodyId))
        {
            QueueAction(() => b2DestroyBody(bodyId));
        }
    }

    public static void Shutdown()
    {
        b2DestroyWorld(WorldId);
        WorldId = default;
    }
}