using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Box2D.NET;
using server.ECS;
using server.Enums;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Worlds;

namespace server.Simulation.Components;

[Component(ComponentType = ComponentType.Box2DBody, NetworkSync = true)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Box2DBodyComponent(B2BodyId bodyId, bool isStatic, uint color,
                         float density = 1f, int sides = 0)
{
    /// <summary>
    /// Tick when this component was last changed, used for network sync.
    /// MUST be first field for raw byte access in ComponentSerializer.
    /// </summary>
    public long ChangedTick = NttWorld.Tick;
    public B2BodyId BodyId = bodyId;
    public bool IsStatic = isStatic;
    public uint Color = color;

    // Store shape information for compatibility with existing systems
    public float Density = density;
    public int Sides = sides;
    internal Vector2 LastPosition;
    internal float LastRotation;

    public readonly bool IsValid => b2Body_IsValid(BodyId);
    // Direct access to Box2D properties
    public readonly Vector2 Position => IsValid ? new Vector2(b2Body_GetTransform(BodyId).p.X, b2Body_GetTransform(BodyId).p.Y) : Vector2.Zero;
    public readonly float Rotation => IsValid ? MathF.Atan2(b2Body_GetTransform(BodyId).q.s, b2Body_GetTransform(BodyId).q.c) : 0f;
    public readonly Vector2 LinearVelocity => IsValid && !IsStatic ? new Vector2(b2Body_GetLinearVelocity(BodyId).X, b2Body_GetLinearVelocity(BodyId).Y) : Vector2.Zero;
    public readonly float AngularVelocity => IsValid && !IsStatic ? b2Body_GetAngularVelocity(BodyId) : 0f;
    public readonly float RotationRadians => Rotation;
    public readonly float Mass => IsValid && !IsStatic ? b2Body_GetMass(BodyId) : 1f;

    public void SetPosition(Vector2 position)
    {
        if (!IsValid)
            return;

        var b2Pos = new B2Vec2(position.X, position.Y);
        var currentTransform = b2Body_GetTransform(BodyId);
        b2Body_SetTransform(BodyId, b2Pos, currentTransform.q);

        // Update ChangedTick to trigger network sync
        ChangedTick = NttWorld.Tick;
    }

    public void SetRotation(float rotation)
    {
        if (!IsValid)
            return;

        var currentTransform = b2Body_GetTransform(BodyId);
        var rot = new B2Rot(MathF.Cos(rotation), MathF.Sin(rotation));
        b2Body_SetTransform(BodyId, currentTransform.p, rot);

        // Update ChangedTick to trigger network sync
        ChangedTick = NttWorld.Tick;
    }

    public readonly void SetLinearVelocity(Vector2 velocity)
    {
        if (!IsValid || IsStatic)
            return;

        var b2Vel = new B2Vec2(velocity.X, velocity.Y);
        b2Body_SetLinearVelocity(BodyId, b2Vel);
    }

    public readonly void SetAngularVelocity(float angularVelocity)
    {
        if (!IsValid || IsStatic)
            return;

        b2Body_SetAngularVelocity(BodyId, angularVelocity);
    }

    public readonly void ApplyForce(Vector2 force, Vector2? point = null)
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

    public readonly void ApplyTorque(float torque)
    {
        if (!IsValid || IsStatic)
            return;

        b2Body_ApplyTorque(BodyId, torque, true);
    }
}