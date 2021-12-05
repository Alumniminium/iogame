using System;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", Environment.ProcessorCount/12) { }

        public override bool MatchesFilter(ref PixelEntity entityId) => false;

        public override void Update(float deltaTime, RefList<PixelEntity> entities)
        {
            
        }
    }
}