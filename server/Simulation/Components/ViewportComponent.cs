using System;
using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public ColliderComponent[] EntitiesVisible = Array.Empty<ColliderComponent>();
        public ColliderComponent[] EntitiesVisibleLastSync = Array.Empty<ColliderComponent>();
        public int ViewDistance;
    }
}