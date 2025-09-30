using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Box2D.NET;
using server.ECS;
using server.Enums;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Worlds;

namespace server.Simulation.Components;

/// <summary>
/// Component representing a Box2D physics body with position, rotation, velocity, and force application.
/// Provides high-performance access to Box2D properties and methods for physics simulation.
/// </summary>
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
    /// <summary>Box2D body identifier for physics engine operations</summary>
    public B2BodyId BodyId = bodyId;
    /// <summary>Whether this body is static (immovable) or dynamic</summary>
    public bool IsStatic = isStatic;
    /// <summary>Visual color for rendering (ARGB format)</summary>
    public uint Color = color;
    /// <summary>Physics density affecting mass calculations</summary>
    public float Density = density;
    /// <summary>Number of sides for shape rendering (0=circle, 3=triangle, 4=box)</summary>
    public int Sides = sides;
    internal Vector2 LastPosition;
    internal float LastRotation;

    /// <summary>Whether the Box2D body reference is still valid</summary>
    public readonly bool IsValid => b2Body_IsValid(BodyId);
    /// <summary>Current world position of the body</summary>
    public readonly Vector2 Position => IsValid ? new Vector2(b2Body_GetTransform(BodyId).p.X, b2Body_GetTransform(BodyId).p.Y) : Vector2.Zero;
    /// <summary>Current rotation in radians</summary>
    public readonly float Rotation => IsValid ? MathF.Atan2(b2Body_GetTransform(BodyId).q.s, b2Body_GetTransform(BodyId).q.c) : 0f;
    /// <summary>Current linear velocity vector</summary>
    public readonly Vector2 LinearVelocity => IsValid && !IsStatic ? new Vector2(b2Body_GetLinearVelocity(BodyId).X, b2Body_GetLinearVelocity(BodyId).Y) : Vector2.Zero;
    /// <summary>Current angular velocity (rotation speed) in radians per second</summary>
    public readonly float AngularVelocity => IsValid && !IsStatic ? b2Body_GetAngularVelocity(BodyId) : 0f;
    /// <summary>Alias for Rotation property</summary>
    public readonly float RotationRadians => Rotation;
    /// <summary>Body mass calculated from density and shape area</summary>
    public readonly float Mass => IsValid && !IsStatic ? b2Body_GetMass(BodyId) : 1f;

    /// <summary>
    /// Sets the body's position and triggers network sync.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        if (!IsValid)
            return;

        var b2Pos = new B2Vec2(position.X, position.Y);
        var currentTransform = b2Body_GetTransform(BodyId);
        b2Body_SetTransform(BodyId, b2Pos, currentTransform.q);

        ChangedTick = NttWorld.Tick;
    }

    /// <summary>
    /// Sets the body's rotation in radians and triggers network sync.
    /// </summary>
    public void SetRotation(float rotation)
    {
        if (!IsValid)
            return;

        var currentTransform = b2Body_GetTransform(BodyId);
        var rot = new B2Rot(MathF.Cos(rotation), MathF.Sin(rotation));
        b2Body_SetTransform(BodyId, currentTransform.p, rot);

        ChangedTick = NttWorld.Tick;
    }

    /// <summary>
    /// Sets the body's linear velocity. Only affects dynamic bodies.
    /// </summary>
    public readonly void SetLinearVelocity(Vector2 velocity)
    {
        if (!IsValid || IsStatic)
            return;

        var b2Vel = new B2Vec2(velocity.X, velocity.Y);
        b2Body_SetLinearVelocity(BodyId, b2Vel);
    }

    /// <summary>
    /// Sets the body's angular velocity (rotation speed). Only affects dynamic bodies.
    /// </summary>
    public readonly void SetAngularVelocity(float angularVelocity)
    {
        if (!IsValid || IsStatic)
            return;

        b2Body_SetAngularVelocity(BodyId, angularVelocity);
    }

    /// <summary>
    /// Applies a force to the body, optionally at a specific world point. Only affects dynamic bodies.
    /// </summary>
    /// <param name="force">Force vector to apply</param>
    /// <param name="point">Optional world point to apply force at (creates torque if offset from center)</param>
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

    /// <summary>
    /// Applies a torque to the body causing rotation. Only affects dynamic bodies.
    /// </summary>
    public readonly void ApplyTorque(float torque)
    {
        if (!IsValid || IsStatic)
            return;

        b2Body_ApplyTorque(BodyId, torque, true);
    }
}