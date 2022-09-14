using System;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public sealed class CleanupSystem : PixelSystem<CollisionComponent>
    {
        public CleanupSystem() : base("Cleanup System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref CollisionComponent col)
        {
            ntt.Remove<CollisionComponent>();
        }
    }
}