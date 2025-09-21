using System;
using System.Collections.Generic;
using Box2D.NET;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Shapes;

namespace server.Simulation.Systems;

public unsafe sealed class ViewportSystem : NttSystem<Box2DBodyComponent, ViewportComponent>
{
    public ViewportSystem() : base("Viewport System", threads: 1) { }
    protected override bool MatchesFilter(in NTT ntt) => base.MatchesFilter(in ntt);

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        // Sync position from Box2D first
        body.SyncFromBox2D();

        // Always update viewport on first tick or periodically
        bool firstUpdate = vwp.EntitiesVisible.Count == 0 && vwp.EntitiesVisibleLast.Count == 0;
        bool periodicUpdate = NttWorld.Tick % 10 == 0; // Update every 10 ticks

        if (!firstUpdate && !periodicUpdate)
            return;
        vwp.Viewport.X = body.Position.X - vwp.Viewport.Width / 2;
        vwp.Viewport.Y = body.Position.Y - vwp.Viewport.Height / 2;

        vwp.EntitiesVisibleLast.Clear();

        vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
        vwp.EntitiesVisible.Clear();

        var entitiesInView = new List<NTT>();

        // Simple viewport culling - check all entities in viewport area
        // This is a simplified approach until we implement proper body ID mapping
        foreach (var entity in NttWorld.NTTs.Values)
        {
            if (entity.Has<Box2DBodyComponent>())
            {
                var entityBody = entity.Get<Box2DBodyComponent>();
                var pos = entityBody.Position;

                // Check if entity is within viewport bounds
                if (pos.X >= vwp.Viewport.X && pos.X <= vwp.Viewport.X + vwp.Viewport.Width &&
                    pos.Y >= vwp.Viewport.Y && pos.Y <= vwp.Viewport.Y + vwp.Viewport.Height)
                {
                    entitiesInView.Add(entity);
                }
            }
        }

        vwp.EntitiesVisible.AddRange(entitiesInView);


        // despawn entities not visible anymore and spawn new ones

        for (var i = 0; i < vwp.EntitiesVisibleLast.Count; i++)
        {
            var b = vwp.EntitiesVisibleLast[i];

            // Skip despawning the viewing entity itself - CRITICAL FIX
            if (ntt.Id == b.Id)
                continue;

            var found = false;
            for (var j = 0; j < vwp.EntitiesVisible.Count; j++)
            {
                found = vwp.EntitiesVisible[j].Id == b.Id;
                if (found)
                    break;
            }

            if (found)
                continue;

            ntt.NetSync(StatusPacket.CreateDespawn(b));
        }

        for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
        {
            var b = vwp.EntitiesVisible[i];
            var found = false;

            for (var j = 0; j < vwp.EntitiesVisibleLast.Count; j++)
            {
                found = vwp.EntitiesVisibleLast[j].Id == b.Id;
                if (found)
                    break;
            }

            if (found)
                continue;

            NetworkHelper.FullSync(ntt, b);
        }
    }
}