using server.ECS;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity entityId) => false;

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {            
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];
            }
        }
    }
}