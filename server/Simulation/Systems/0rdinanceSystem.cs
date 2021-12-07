using server.ECS;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", Environment.ProcessorCount/12) { }

        protected override bool MatchesFilter(ref PixelEntity entityId) => false;

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {
            
        }
    }
}