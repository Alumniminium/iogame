using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class SpawnSystem : PixelSystem<PositionComponent, SpawnerComponent>
    {
        public SpawnSystem() : base("Spawn System", threads: 1) { }

        protected override void Update(float dt, List<PixelEntity> entities)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                ref var spwn = ref entity.Get<SpawnerComponent>();
                ref readonly var pos = ref entity.Get<PositionComponent>();

                spwn.TimeSinceLastSpawn += dt;

                switch (spwn.UnitIdToSpawn)
                {
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        {
                            var population = PixelWorld.Triangles.Count;

                            if (population < spwn.MinPopulation)
                            {
                                SpawnManager.Spawn(Db.BaseResources[spwn.UnitIdToSpawn], pos.Position, Vector2.Normalize(SpawnManager.GetRandomVelocity()) * 10);
                                break;
                            }

                            if (population < spwn.MaxPopulation)
                            {
                                if (spwn.Interval.TotalMilliseconds <= spwn.TimeSinceLastSpawn)
                                {
                                    spwn.TimeSinceLastSpawn = 0;
                                    for (int x = 0; x < spwn.AmountPerInterval; x++)
                                    {
                                        SpawnManager.Spawn(Db.BaseResources[spwn.UnitIdToSpawn], pos.Position, Vector2.Normalize(SpawnManager.GetRandomVelocity()) * 10);
                                    }
                                }
                                break;
                            }

                            break;
                        }
                }
            }
        }
    }
}