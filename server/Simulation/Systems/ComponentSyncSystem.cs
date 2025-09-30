using System;
using server.ECS;
using server.Helpers;
using server.Serialization;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// System that automatically syncs components to clients using the generic ComponentSerializer.
/// This replaces the old NetSyncSystem's manual component handling.
/// </summary>
public sealed class ComponentSyncSystem : NttSystem<NetworkComponent, ViewportComponent>
{
    public ComponentSyncSystem() : base("Component Sync System", threads: 1) { }

    public override void Update(in NTT ntt, ref NetworkComponent network, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        foreach (var visibleEntity in vwp.EntitiesVisible)
        {
            SyncEntity(ntt, visibleEntity);
            SyncChildEntities(ntt, visibleEntity);
        }

        SyncEntity(ntt, ntt);
        SyncChildEntities(ntt, ntt);
    }

    /// <summary>
    /// Syncs all network-enabled components from the specified entity to the viewer.
    /// </summary>
    private static void SyncEntity(NTT viewer, NTT entity)
    {
        foreach (var componentType in ComponentSerializer.NetworkSyncTypes)
            ComponentSerializer.TrySyncComponent(viewer, entity, componentType);
    }

    /// <summary>
    /// Syncs all child entities (e.g., ship parts) of a parent entity to the viewer.
    /// Uses efficient packed storage iteration to find children.
    /// </summary>
    private static void SyncChildEntities(NTT viewer, NTT parentEntity)
    {
        var components = PackedComponentStorage<ParentChildComponent>.GetComponentSpan();
        var entities = PackedComponentStorage<ParentChildComponent>.GetEntitySpan();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].ParentId == parentEntity)
            {
                var childEntity = new NTT(entities[i]);
                SyncEntity(viewer, childEntity);
            }
        }
    }
}