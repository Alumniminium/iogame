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

        // Debug logging
        if (NttWorld.Tick % 60 == 0)
            FConsole.WriteLine($"[ComponentSync] Player {ntt.Id}: {vwp.EntitiesVisible.Count} visible entities");

        // Sync all visible entities
        foreach (var visibleEntity in vwp.EntitiesVisible)
        {
            SyncEntity(ntt, visibleEntity);

            // Also sync any child entities (ship parts) of visible entities
            SyncChildEntities(ntt, visibleEntity);
        }

        // Also sync self
        SyncEntity(ntt, ntt);

        // Sync self's child entities (ship parts)
        SyncChildEntities(ntt, ntt);
    }

    private static void SyncEntity(NTT viewer, NTT entity)
    {
        foreach (var componentType in ComponentSerializer.NetworkSyncTypes)
            ComponentSerializer.TrySyncComponent(viewer, entity, componentType);
    }

    private static void SyncChildEntities(NTT viewer, NTT parentEntity)
    {
        // Efficiently iterate only entities that have ParentChildComponent using parallel spans
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