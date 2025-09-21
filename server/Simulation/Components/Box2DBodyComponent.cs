using System;
using System.Numerics;
using Box2D.NET;
using server.ECS;
using server.Enums;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Ids;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;

namespace server.Simulation.Components;

[Component]
public struct Box2DBodyComponent
{
    public readonly NTT EntityId;
    public B2BodyId BodyId;
    public bool IsStatic;
    public uint Color;

    // Store shape information for compatibility with existing systems
    public ShapeType ShapeType;
    public float Density;
    public int Sides;

    // Cache frequently accessed properties to avoid B2 API calls
    public Vector2 Position;
    public float Rotation;
    public Vector2 LinearVelocity;
    public float AngularVelocity;

    // Compatibility properties for old physics system
    public Vector2 LastPosition;
    public float LastRotation;
    public float RotationRadians => Rotation;

    // Track if we need to sync from Box2D
    public bool NeedsSync;
    public long LastSyncTick;

    public Box2DBodyComponent(NTT entityId, B2BodyId bodyId, bool isStatic, uint color,
                             ShapeType shapeType = ShapeType.Circle, float density = 1f, int sides = 0)
    {
        EntityId = entityId;
        BodyId = bodyId;
        IsStatic = isStatic;
        Color = color;
        ShapeType = shapeType;
        Density = density;
        Sides = sides;
        Position = Vector2.Zero;
        Rotation = 0f;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        LastPosition = Vector2.Zero;
        LastRotation = 0f;
        NeedsSync = true;
        LastSyncTick = 0;
    }

    public readonly bool IsValid => b2Body_IsValid(BodyId);

    public void SyncFromBox2D()
    {
        if (!IsValid)
            return;

        // Store previous values
        LastPosition = Position;
        LastRotation = Rotation;

        var transform = b2Body_GetTransform(BodyId);
        Position = new Vector2(transform.p.X, transform.p.Y);
        Rotation = MathF.Atan2(transform.q.s, transform.q.c);

        if (!IsStatic)
        {
            var velocity = b2Body_GetLinearVelocity(BodyId);
            LinearVelocity = new Vector2(velocity.X, velocity.Y);
            AngularVelocity = b2Body_GetAngularVelocity(BodyId);
        }

        NeedsSync = false;
        LastSyncTick = NttWorld.Tick;
    }

    public void SetPosition(Vector2 position)
    {
        if (!IsValid)
            return;

        var b2Pos = new B2Vec2(position.X, position.Y);
        var currentTransform = b2Body_GetTransform(BodyId);
        b2Body_SetTransform(BodyId, b2Pos, currentTransform.q);
        Position = position;
    }

    public void SetRotation(float rotation)
    {
        if (!IsValid)
            return;

        var currentTransform = b2Body_GetTransform(BodyId);
        var rot = new B2Rot(MathF.Cos(rotation), MathF.Sin(rotation));
        b2Body_SetTransform(BodyId, currentTransform.p, rot);
        Rotation = rotation;
    }

    public void SetLinearVelocity(Vector2 velocity)
    {
        if (!IsValid || IsStatic)
            return;

        var b2Vel = new B2Vec2(velocity.X, velocity.Y);
        b2Body_SetLinearVelocity(BodyId, b2Vel);
        LinearVelocity = velocity;
    }

    public void ApplyForce(Vector2 force, Vector2? point = null)
    {
        if (!IsValid || IsStatic)
            return;

        var b2Force = new B2Vec2(force.X, force.Y);

        if (point.HasValue)
        {
            var b2Point = new B2Vec2(point.Value.X, point.Value.Y);
            b2Body_ApplyForce(BodyId, b2Force, b2Point, true);
        }
        else
        {
            b2Body_ApplyForceToCenter(BodyId, b2Force, true);
        }
    }

    public void ApplyTorque(float torque)
    {
        if (!IsValid || IsStatic)
            return;

        b2Body_ApplyTorque(BodyId, torque, true);
    }
}