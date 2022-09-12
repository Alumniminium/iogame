using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref LifeTimeComponent c1)
        {
            c1.LifeTimeSeconds -= deltaTime;

            if (c1.LifeTimeSeconds <= 0)
                PixelWorld.Destroy(in ntt);
        }
    }
}