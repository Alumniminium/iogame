using System;
using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using static Box2D.NET.B2Bodies;

namespace server.Simulation.Systems;

/// <summary>
/// System that rebuilds physics bodies when ship parts are added or changed
/// </summary>
public sealed class ShipPhysicsRebuildSystem : NttSystem<PhysicsComponent, NetworkComponent>
{
    private readonly Dictionary<NTT, long> lastRebuildTick = [];

    public ShipPhysicsRebuildSystem() : base("Ship Physics Rebuild System", threads: 1) { }

    public override void Update(in NTT ntt, ref PhysicsComponent body, ref NetworkComponent network)
    {
        if (!HasShipParts(ntt))
            return;

        var latestPartChange = GetLatestPartChangeTick(ntt);
        if (latestPartChange == 0)
            return;

        // Skip if we already rebuilt for this change
        if (lastRebuildTick.TryGetValue(ntt, out var lastTick) && lastTick >= latestPartChange)
            return;

        RebuildPhysicsBody(ntt, ref body);
        lastRebuildTick[ntt] = latestPartChange; // Store the part change tick, not current tick
    }

    private static bool HasShipParts(NTT entity)
    {
        foreach (var child in entity.GetChildren())
        {
            ref var pc = ref child.Get<ParentChildComponent>();
            if (pc.Shape > 0)
                return true;
        }
        return false;
    }

    private static long GetLatestPartChangeTick(NTT entity)
    {
        long latestTick = 0;

        foreach (var child in entity.GetChildren())
        {
            ref var pc = ref child.Get<ParentChildComponent>();
            if (pc.Shape > 0)
                latestTick = Math.Max(latestTick, pc.ChangedTick);
        }
        return latestTick;
    }

    /// <summary>
    /// Rebuilds the physics body for a ship when parts are added/removed.
    /// This involves destroying the old Box2D body and creating a new compound body with all shapes.
    /// Critical: Position, velocity, and ChangedTick must be preserved to avoid client sync issues.
    /// </summary>
    private static void RebuildPhysicsBody(NTT entity, ref PhysicsComponent body)
    {
        if (!body.IsValid)
            return;

        // Save all properties that must be preserved across rebuild
        // Use LastPosition/LastRotation instead of querying Box2D directly
        // because this system runs before physics step, so Box2D data might be stale
        var currentPos = body.LastPosition;
        var currentRot = body.LastRotation;
        var currentVel = body.LinearVelocity;
        var currentAngVel = body.AngularVelocity;
        var originalDensity = body.Density;
        var originalColor = body.Color;

        // CRITICAL: Preserve ChangedTick to prevent false-positive change detection
        // When we create a new PhysicsComponent, its constructor initializes ChangedTick to default (0).
        // ComponentSyncSystem detects changes by comparing ChangedTick values between frames.
        // If we don't restore the original ChangedTick, the system sees: old=X, new=0, and syncs to clients.
        // This causes camera jumps because clients receive position updates even though position hasn't changed.
        var originalChangedTick = body.ChangedTick;

        // Players have special collision groups
        var isPlayer = entity.Has<NetworkComponent>();
        uint categoryBits = isPlayer ? (uint)Enums.CollisionCategory.Player : 0x0001;
        uint maskBits = isPlayer ? (uint)Enums.CollisionCategory.All : 0xFFFF;
        int groupIndex = isPlayer ? -(Math.Abs(entity.Id.GetHashCode()) % 1000 + 1) : 0;

        // Destroy the old Box2D body (can't modify shape of existing body)
        b2DestroyBody(body.BodyId);

        // Build list of shapes for the compound body
        var shapes = new List<(Vector2 offset, Enums.ShapeType shapeType, float shapeRotation)>
        {
            // Add the core player box at origin (0,0)
            (Vector2.Zero, Enums.ShapeType.Box, 0f)
        };

        // Collect all ship parts attached to this entity
        foreach (var child in entity.GetChildren())
        {
            ref var parentChild = ref child.Get<ParentChildComponent>();
            if (parentChild.Shape > 0)
            {
                // Grid position becomes the local offset in the compound body
                var offset = new Vector2(parentChild.GridX, parentChild.GridY);

                var shapeType = parentChild.Shape switch
                {
                    1 => Enums.ShapeType.Triangle,
                    2 => Enums.ShapeType.Box,
                    _ => Enums.ShapeType.Box
                };

                // Convert rotation (0-3 representing 0°, 90°, 180°, 270°) to radians
                var rotationRad = parentChild.Rotation * MathF.PI / 2f;

                shapes.Add((offset, shapeType, rotationRad));
            }
        }

        // Create new compound Box2D body with all shapes at the same position/rotation
        var (newBodyId, localCenter) = PhysicsWorld.CreateCompoundBody(
            currentPos,
            currentRot,
            body.IsStatic,
            shapes,
            originalDensity,
            0.1f,  // drag
            0.2f,  // elasticity
            categoryBits,
            maskBits,
            groupIndex,
            isPlayer);

        // Replace component with new body ID
        body = new PhysicsComponent(newBodyId, body.IsStatic, originalColor, originalDensity, body.Sides);

        // Restore physics state (velocity, etc.)
        if (!body.IsStatic)
        {
            body.SetLinearVelocity(currentVel);
            body.SetAngularVelocity(currentAngVel);
        }

        // CRITICAL: Restore the original ChangedTick
        // The new PhysicsComponent constructor sets ChangedTick=0 by default.
        // If we leave it at 0, ComponentSyncSystem sees a change (oldTick != 0) and syncs to clients.
        // This would send position updates even though position hasn't changed, causing camera jumps.
        // By restoring the original tick, we tell the sync system "nothing changed, don't sync".
        body.ChangedTick = originalChangedTick;
    }
}