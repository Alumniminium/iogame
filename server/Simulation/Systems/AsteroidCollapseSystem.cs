using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public class AsteroidCollapseSystem : NttSystem<StructuralCollapseComponent>
{
    public AsteroidCollapseSystem() : base("Asteroid Collapse", threads: 1) { }
    public override void Update(in NTT ntt, ref StructuralCollapseComponent collapse)
    {
        // Mark entity for death - this will trigger the existing death systems
        ntt.Set(new DeathTagComponent());

        // Remove the collapse component since we've processed it
        ntt.Remove<StructuralCollapseComponent>();
    }
}