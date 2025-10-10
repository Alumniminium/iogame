using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// System that marks Box2DBodyComponent as changed when position/velocity changes.
/// This ensures ComponentSyncSystem will sync the physics data to clients.
/// </summary>
public sealed class PositionSyncSystem : NttSystem<NetworkComponent, ViewportComponent>
{
    public PositionSyncSystem() : base("Position Sync System", threads: 1) { }

    public override void Update(in NTT ntt, ref NetworkComponent network, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        // Mark position changes for all visible entities
        foreach (var visibleEntity in vwp.EntitiesVisible)
            MarkPositionChanges(visibleEntity);

        // Also mark self
        MarkPositionChanges(ntt);
    }

    private static void MarkPositionChanges(NTT ntt)
    {
        if (!ntt.Has<Box2DBodyComponent>())
            return;

        ref var rigidBody = ref ntt.Get<Box2DBodyComponent>();

        // Skip if no change or very small change
        var positionDelta = rigidBody.Position - rigidBody.LastPosition;
        if (positionDelta.LengthSquared() < 0.01f && MathF.Abs(rigidBody.RotationRadians - rigidBody.LastRotation) < 0.01f)
            return;

        rigidBody.LastRotation = rigidBody.RotationRadians;
        rigidBody.LastPosition = rigidBody.Position;
        rigidBody.ChangedTick = NttWorld.Tick;
    }
}