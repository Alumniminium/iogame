using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class SpawnSystem : PixelSystem<SpawnerComponent>
    {
        public SpawnSystem() : base("Spawn System", 1) { }

        protected override void Update(float deltaTime, List<PixelEntity> entities)
        {            
            for (var i = 0; i < entities.Count; i++)
            {
                var entity =  entities[i];

                ref readonly var spwn = ref entity.Get<SpawnerComponent>();

                switch (spwn.UnitIdToSpawn)
                {
                    case 4: // Square
                    {
                        // var population = 
                        break;
                    }
                }
            }
        }
    }
}