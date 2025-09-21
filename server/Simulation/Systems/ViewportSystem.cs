using System;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

public unsafe sealed class ViewportSystem : NttSystem<PhysicsComponent, ViewportComponent>
{
    public ViewportSystem() : base("Viewport System", threads: 1) { }
    protected override bool MatchesFilter(in NTT ntt) => base.MatchesFilter(in ntt);

    public override void Update(in NTT ntt, ref PhysicsComponent phy, ref ViewportComponent vwp)
    {
        if (phy.LastPosition == phy.Position || !ntt.Has<NetworkComponent>())
            return;

        vwp.Viewport.X = phy.Position.X - vwp.Viewport.Width / 2;
        vwp.Viewport.Y = phy.Position.Y - vwp.Viewport.Height / 2;

        vwp.EntitiesVisibleLast.Clear();

        vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
        vwp.EntitiesVisible.Clear();

        Game.Grid.GetVisibleEntities(ref vwp);

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