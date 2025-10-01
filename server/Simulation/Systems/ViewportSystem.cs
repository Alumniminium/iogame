using System.Collections.Generic;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

/// <summary>
/// Manages client viewports by determining which entities are visible to each player.
/// Performs AABB intersection tests to cull entities outside the viewport for network optimization.
/// Updates every 10 ticks to reduce CPU overhead while maintaining responsive visibility.
/// </summary>
public unsafe sealed class ViewportSystem : NttSystem<Box2DBodyComponent, ViewportComponent>
{
    public ViewportSystem() : base("Viewport System", threads: 1) { }
    protected override bool MatchesFilter(in NTT ntt) => base.MatchesFilter(in ntt);

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref ViewportComponent vwp)
    {
        if (!ntt.Has<NetworkComponent>())
            return;

        bool firstUpdate = vwp.EntitiesVisible.Count == 0 && vwp.EntitiesVisibleLast.Count == 0;
        bool periodicUpdate = NttWorld.Tick % 10 == 0;

        if (!firstUpdate && !periodicUpdate)
            return;
        vwp.Viewport.X = body.Position.X - vwp.Viewport.Width / 2;
        vwp.Viewport.Y = body.Position.Y - vwp.Viewport.Height / 2;

        vwp.EntitiesVisibleLast.Clear();

        vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
        vwp.EntitiesVisible.Clear();

        var entitiesInView = new List<NTT>();

        foreach (var entity in NttQuery.Query<Box2DBodyComponent>())
        {
            var entityBody = entity.Get<Box2DBodyComponent>();
            var pos = entityBody.Position;

            var halfWidth = 0.5f;
            var halfHeight = 0.5f;

            var entityMinX = pos.X - halfWidth;
            var entityMaxX = pos.X + halfWidth;
            var entityMinY = pos.Y - halfHeight;
            var entityMaxY = pos.Y + halfHeight;

            var viewportMinX = vwp.Viewport.X;
            var viewportMaxX = vwp.Viewport.X + vwp.Viewport.Width;
            var viewportMinY = vwp.Viewport.Y;
            var viewportMaxY = vwp.Viewport.Y + vwp.Viewport.Height;

            if (entityMaxX >= viewportMinX && entityMinX <= viewportMaxX &&
                entityMaxY >= viewportMinY && entityMinY <= viewportMaxY)
                entitiesInView.Add(entity);
        }

        vwp.EntitiesVisible.AddRange(entitiesInView);
    }
}