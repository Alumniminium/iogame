using System.Collections.Generic;
using server.ECS;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", 1) { }

        protected override bool MatchesFilter(ref PixelEntity entityId) => false;

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {            
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
            }
        }
    }
}