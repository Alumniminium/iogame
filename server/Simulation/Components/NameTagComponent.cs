using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public readonly struct NameTagComponent
    {
        public readonly string Name;

        public NameTagComponent(string name) => Name = name;
    }
}