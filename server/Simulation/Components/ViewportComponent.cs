using iogame.ECS;
using iogame.Simulation.Entities;

namespace iogame.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public ColliderComponent[] EntitiesVisible = Array.Empty<ColliderComponent>();
        public ColliderComponent[] EntitiesVisibleLastSync = Array.Empty<ColliderComponent>();
        public int ViewDistance;
    }
}