using iogame.ECS;
using iogame.Simulation.Entities;

namespace iogame.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public List<ShapeEntity> EntitiesVisible = new();
        public List<ShapeEntity> EntitiesVisibleLastSync = new();
        public int ViewDistance { get; internal set; }
    }
}