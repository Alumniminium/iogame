using System;
using System.Collections.Generic;
using System.Numerics;
using Box2D.NET;
using server.ECS;
using server.Enums;
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

    static Box2DPhysicsWorld() => Initialize();

    private static void Initialize()
    {
        var worldDef = b2DefaultWorldDef();
        worldDef.gravity = new B2Vec2(0, 0); // No global gravity - using custom gravity sources
        worldDef.enableSleep = true;
        worldDef.workerCount = 16; // Multi-threaded Box2D
        WorldId = b2CreateWorld(ref worldDef);
    }

    public static void Step(float deltaTime, int subStepCount = 0)
    {
        b2World_Step(WorldId, deltaTime, subStepCount);

        // Check for collisions using contact manifolds instead of events
        CheckAllContacts();
    }

    private static void CheckAllContacts()
    {
        // Process contact begin touch events using the correct Box2D.NET API
        var contactEvents = b2World_GetContactEvents(WorldId);

        for (int i = 0; i < contactEvents.beginCount; i++)
        {
            var beginEvent = contactEvents.beginEvents[i];
            var bodyIdA = b2Shape_GetBody(beginEvent.shapeIdA);
            var bodyIdB = b2Shape_GetBody(beginEvent.shapeIdB);

            // Find entities corresponding to these Box2D bodies
            var entityA = FindEntityByBodyId(bodyIdA);
            var entityB = FindEntityByBodyId(bodyIdB);

            if (entityA.HasValue && entityB.HasValue)
            {
                var nttA = entityA.Value;
                var nttB = entityB.Value;

                // Debug what types are colliding
                var typeA = nttA.Has<BulletComponent>() ? "Bullet" : nttA.Has<NetworkComponent>() ? "Player" : "Other";
                var typeB = nttB.Has<BulletComponent>() ? "Bullet" : nttB.Has<NetworkComponent>() ? "Player" : "Other";
                // Add collision component to both entities
                AddCollisionToEntity(nttA, nttB, beginEvent.manifold);
                AddCollisionToEntity(nttB, nttA, beginEvent.manifold);
            }
        }

        // Process sensor events and add them as CollisionComponent events
        // This allows existing pickup systems to work with sensor-based drops
        var sensorEvents = b2World_GetSensorEvents(WorldId);


        for (int i = 0; i < sensorEvents.beginCount; i++)
        {
            var sensorEvent = sensorEvents.beginEvents[i];
            var sensorBodyId = b2Shape_GetBody(sensorEvent.sensorShapeId);
            var visitorBodyId = b2Shape_GetBody(sensorEvent.visitorShapeId);

            var sensorEntity = FindEntityByBodyId(sensorBodyId);
            var visitorEntity = FindEntityByBodyId(visitorBodyId);

            if (sensorEntity.HasValue && visitorEntity.HasValue)
            {
                var sensor = sensorEntity.Value;
                var visitor = visitorEntity.Value;

                // Convert sensor events to collision events for existing pickup system
                if (sensor.Has<PickableTagComponent>() && visitor.Has<NetworkComponent>())
                {

                    // Create a dummy manifold for pickup detection
                    var dummyManifold = new B2Manifold
                    {
                        normal = new B2Vec2(0, 0),
                        pointCount = 1
                    };
                    dummyManifold.points[0].separation = 0f;

                    AddCollisionToEntity(visitor, sensor, dummyManifold);
                }
            }
        }

    }

    private static NTT? FindEntityByBodyId(B2BodyId bodyId)
    {
        // Search through all entities with Box2DBodyComponent to find matching bodyId
        foreach (var entity in NttQuery.Query<Box2DBodyComponent>())
        {
            var body = entity.Get<Box2DBodyComponent>();
            if (body.BodyId.index1 == bodyId.index1)
                return entity;
        }
        return null;
    }

    private static void AddCollisionToEntity(NTT entity, NTT otherEntity, B2Manifold manifold)
    {
        CollisionComponent collision;

        if (entity.Has<CollisionComponent>())
        {
            collision = entity.Get<CollisionComponent>();
        }
        else
        {
            collision = new CollisionComponent(entity);
            entity.Set(ref collision);
        }

        // Convert Box2D manifold to our collision data
        var contactPoint = new Vector2(manifold.normal.X, manifold.normal.Y);
        var penetration = manifold.pointCount > 0 ? manifold.points[0].separation : 0f;

        collision.Collisions.Add((otherEntity, contactPoint, penetration));
    }


    public static B2BodyId CreateBody(Vector2 position, float rotation, bool isStatic, ShapeType shapeType, float density = 1f, float friction = 0.3f, float restitution = 0.2f, uint categoryBits = 0x0001, uint maskBits = 0xFFFF, int groupIndex = 0, bool isSensor = false, bool enableSensorEvents = false)
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
        shapeDef.filter.categoryBits = categoryBits;
        shapeDef.filter.maskBits = maskBits;
        shapeDef.filter.groupIndex = groupIndex;
        shapeDef.isSensor = isSensor;
        shapeDef.enableSensorEvents = enableSensorEvents;
        shapeDef.enableContactEvents = !isSensor; // Enable collision events only for non-sensors

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
                    new(0, -0.5f),    // Top
                    new(-0.5f, 0.5f), // Bottom left
                    new(0.5f, 0.5f)   // Bottom right
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

    public static B2BodyId CreateCircleBody(Vector2 position, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f, uint categoryBits = 0x0001, uint maskBits = 0xFFFF, int groupIndex = 0)
    {
        return CreateBody(position, 0f, isStatic, ShapeType.Circle, density, friction, restitution, categoryBits, maskBits, groupIndex);
    }

    public static B2BodyId CreateBoxBody(Vector2 position, float rotation, bool isStatic, float density = 1f, float friction = 0.3f, float restitution = 0.2f, uint categoryBits = 0x0001, uint maskBits = 0xFFFF, int groupIndex = 0, bool enableSensorEvents = false)
    {
        return CreateBody(position, rotation, isStatic, ShapeType.Box, density, friction, restitution, categoryBits, maskBits, groupIndex, false, enableSensorEvents);
    }

    public static void DestroyBody(B2BodyId bodyId)
    {
        if (b2Body_IsValid(bodyId))
            b2DestroyBody(bodyId);
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

    }

    public static (B2BodyId, Vector2) CreateCompoundBody(Vector2 position, float rotation, bool isStatic, List<(Vector2 offset, ShapeType shapeType, float shapeRotation)> shapes, float density = 1f, float friction = 0.3f, float restitution = 0.2f, uint categoryBits = 0x0001, uint maskBits = 0xFFFF, int groupIndex = 0, bool enableSensorEvents = false)
    {
        var bodyDef = b2DefaultBodyDef();
        bodyDef.type = isStatic ? B2BodyType.b2_staticBody : B2BodyType.b2_dynamicBody;
        bodyDef.position = new B2Vec2(position.X, position.Y);
        bodyDef.rotation = new B2Rot(MathF.Cos(rotation), MathF.Sin(rotation));
        bodyDef.enableSleep = true;
        bodyDef.angularDamping = 0.01f;
        bodyDef.linearDamping = 0.1f;

        var bodyId = b2CreateBody(WorldId, ref bodyDef);

        // Create shape definition (defer mass calculation until all shapes are added)
        var shapeDef = b2DefaultShapeDef();
        shapeDef.density = density;
        shapeDef.material.friction = friction;
        shapeDef.material.restitution = restitution;
        shapeDef.filter.categoryBits = categoryBits;
        shapeDef.filter.maskBits = maskBits;
        shapeDef.filter.groupIndex = groupIndex;
        shapeDef.isSensor = false;
        shapeDef.enableSensorEvents = enableSensorEvents;
        shapeDef.enableContactEvents = true;
        shapeDef.updateBodyMass = false; // Defer mass calculation

        foreach (var (offset, shapeType, shapeRotation) in shapes)
        {
            // Create rotation from shape rotation (in radians)
            var shapeRot = new B2Rot(MathF.Cos(shapeRotation), MathF.Sin(shapeRotation));

            switch (shapeType)
            {
                case ShapeType.Box:
                    var box = b2MakeOffsetBox(0.5f, 0.5f, new B2Vec2(offset.X, offset.Y), shapeRot);
                    b2CreatePolygonShape(bodyId, ref shapeDef, ref box);
                    break;

                case ShapeType.Circle:
                    // Circles don't need rotation, but we still offset them
                    var circle = new B2Circle(new B2Vec2(offset.X, offset.Y), 0.5f);
                    b2CreateCircleShape(bodyId, ref shapeDef, ref circle);
                    break;

                case ShapeType.Triangle:
                    // Create triangle vertices (pointing up by default)
                    var trianglePoints = new B2Vec2[3]
                    {
                        new(0f, -0.5f),    // Top
                        new(-0.5f, 0.5f),  // Bottom left
                        new(0.5f, 0.5f)    // Bottom right
                    };

                    // Apply rotation and offset to each point
                    var cos = MathF.Cos(shapeRotation);
                    var sin = MathF.Sin(shapeRotation);
                    for (int i = 0; i < trianglePoints.Length; i++)
                    {
                        var x = trianglePoints[i].X;
                        var y = trianglePoints[i].Y;
                        trianglePoints[i] = new B2Vec2(
                            offset.X + x * cos - y * sin,
                            offset.Y + x * sin + y * cos
                        );
                    }

                    var triangleHull = b2ComputeHull(trianglePoints.AsSpan(), 3);
                    var triangle = b2MakePolygon(ref triangleHull, 0f);
                    b2CreatePolygonShape(bodyId, ref shapeDef, ref triangle);
                    break;

                default:
                    // Default to box
                    var defaultBox = b2MakeOffsetBox(0.5f, 0.5f, new B2Vec2(offset.X, offset.Y), shapeRot);
                    b2CreatePolygonShape(bodyId, ref shapeDef, ref defaultBox);
                    break;
            }
        }

        // Apply mass properties after all shapes are added
        b2Body_ApplyMassFromShapes(bodyId);

        var localCenterB2 = b2Body_GetLocalCenterOfMass(bodyId);
        var localCenter = new Vector2(localCenterB2.X, localCenterB2.Y);

        return (bodyId, localCenter);
    }

}