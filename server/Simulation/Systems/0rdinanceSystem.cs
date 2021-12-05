using iogame.ECS;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", Environment.ProcessorCount) { }

        public override bool MatchesFilter(ref PixelEntity entityId) => false;

        public override void Update(float deltaTime, List<PixelEntity> Entities)
        {
            
        }
    }
}