using System;
using System.Collections.Generic;
using server.ECS;
using server.Helpers;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", Environment.ProcessorCount) { }

        protected override bool MatchesFilter(ref PixelEntity entityId) => false;

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {
            
        }
    }
}