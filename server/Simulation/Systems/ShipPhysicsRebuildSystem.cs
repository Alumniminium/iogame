using System;
using System.Collections.Generic;
using System.Numerics;
using Box2D.NET;
using server.ECS;
using server.Simulation.Components;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Hulls;

namespace server.Simulation.Systems;

/// <summary>
/// System that rebuilds physics bodies when ship parts are added or changed
/// </summary>
public sealed class ShipPhysicsRebuildSystem : NttSystem<Box2DBodyComponent, NetworkComponent>
{
    private readonly Dictionary<NTT, long> lastRebuildTick = [];

    public ShipPhysicsRebuildSystem() : base("Ship Physics Rebuild System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref NetworkComponent network)
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
        var components = PackedComponentStorage<ParentChildComponent>.GetComponentSpan();
        var entities = PackedComponentStorage<ParentChildComponent>.GetEntitySpan();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].ParentId == entity)
            {
                var childEntity = new NTT(entities[i]);
                if (childEntity.Has<ShipPartComponent>())
                    return true;
            }
        }
        return false;
    }

    private static long GetLatestPartChangeTick(NTT entity)
    {
        long latestTick = 0;
        var components = PackedComponentStorage<ParentChildComponent>.GetComponentSpan();
        var entities = PackedComponentStorage<ParentChildComponent>.GetEntitySpan();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].ParentId == entity)
            {
                var childEntity = new NTT(entities[i]);
                if (childEntity.Has<ShipPartComponent>())
                {
                    var shipPart = childEntity.Get<ShipPartComponent>();
                    latestTick = Math.Max(latestTick, shipPart.ChangedTick);
                }
            }
        }
        return latestTick;
    }

    private static void RebuildPhysicsBody(NTT entity, ref Box2DBodyComponent body)
    {
        if (!body.IsValid)
            return;

        // Use LastPosition/LastRotation instead of querying Box2D directly
        // because this system runs before physics step, so Box2D data might be stale
        var currentPos = body.LastPosition;
        var currentRot = body.LastRotation;
        var currentVel = body.LinearVelocity;
        var currentAngVel = body.AngularVelocity;
        var originalDensity = body.Density;
        var originalColor = body.Color;

        // Players have special collision groups
        var isPlayer = entity.Has<NetworkComponent>();
        uint categoryBits = isPlayer ? (uint)Enums.CollisionCategory.Player : 0x0001;
        uint maskBits = isPlayer ? (uint)Enums.CollisionCategory.All : 0xFFFF;
        int groupIndex = isPlayer ? -(Math.Abs(entity.Id.GetHashCode()) % 1000 + 1) : 0;

        b2DestroyBody(body.BodyId);

        var shapes = new List<(Vector2 offset, Enums.ShapeType shapeType, float shapeRotation)>
        {
            // Add the original player box at origin (0,0)
            (Vector2.Zero, Enums.ShapeType.Box, 0f)
        };

        var components = PackedComponentStorage<ParentChildComponent>.GetComponentSpan();
        var entities = PackedComponentStorage<ParentChildComponent>.GetEntitySpan();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].ParentId == entity)
            {
                var childEntity = new NTT(entities[i]);
                if (childEntity.Has<ShipPartComponent>())
                {
                    var shipPart = childEntity.Get<ShipPartComponent>();

                    var offset = new Vector2(shipPart.GridX, shipPart.GridY);

                    var shapeType = shipPart.Shape switch
                    {
                        1 => Enums.ShapeType.Triangle,
                        2 => Enums.ShapeType.Box,
                        _ => Enums.ShapeType.Box
                    };

                    // Convert rotation (0-3 representing 0°, 90°, 180°, 270°) to radians
                    var rotationRad = shipPart.Rotation * MathF.PI / 2f;

                    shapes.Add((offset, shapeType, rotationRad));
                }
            }
        }

        var (newBodyId, localCenter) = Box2DPhysicsWorld.CreateCompoundBody(
            currentPos,
            currentRot,
            body.IsStatic,
            shapes,
            originalDensity,
            0.1f,
            0.2f,
            categoryBits,
            maskBits,
            groupIndex,
            isPlayer);

        body = new Box2DBodyComponent(newBodyId, body.IsStatic, originalColor, originalDensity, body.Sides);

        if (!body.IsStatic)
        {
            body.SetLinearVelocity(currentVel);
            body.SetAngularVelocity(currentAngVel);
        }

        body.ChangedTick = NttWorld.Tick;
    }
}