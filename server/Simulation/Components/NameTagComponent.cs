using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct NameTagComponent
    {
        public readonly int EntityId;
        public readonly string Name;

        public NameTagComponent(int entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
        public override int GetHashCode() => EntityId;
    }
}