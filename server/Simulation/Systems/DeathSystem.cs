using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent ded)
        {
            PixelWorld.Destroy(in ntt);
        }
    }
}