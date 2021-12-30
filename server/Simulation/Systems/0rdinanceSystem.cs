using server.ECS;

namespace server.Simulation.Systems
{
    public class OrdinanceSystem : PixelSystem
    {
        public OrdinanceSystem() : base("Ordinance System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity entityId) => false;

        protected override void Update(float deltaTime, Span<PixelEntity> entities)
        {            
            for (var i = 0; i < entities.Length; i++)
            {
                ref var entity =  ref entities[i];
            }
        }
    }
}