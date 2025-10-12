using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// System that marks PhysicsComponent as changed when position/velocity changes.
/// This ensures ComponentSyncSystem will sync the physics data to clients.
/// </summary>
public sealed class PositionSyncSystem : NttSystem<NetworkComponent, ViewportComponent>
{
    public PositionSyncSystem() : base("Position Sync System", threads: 1) { }

    public override void Update(in NTT ntt, ref NetworkComponent network, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        foreach (var visibleNtt in vwp.EntitiesVisible)
        {
            if (!visibleNtt.Has<PhysicsComponent>())
                continue;

            ref var hisBody = ref visibleNtt.Get<PhysicsComponent>();
            hisBody.LastRotation = hisBody.RotationRadians;
            hisBody.LastPosition = hisBody.Position;
            hisBody.ChangedTick = NttWorld.Tick;
        }

        if (!ntt.Has<PhysicsComponent>())
            return;

        ref var rigidBody = ref ntt.Get<PhysicsComponent>();
        rigidBody.LastRotation = rigidBody.RotationRadians;
        rigidBody.LastPosition = rigidBody.Position;
        rigidBody.ChangedTick = NttWorld.Tick;
    }
}