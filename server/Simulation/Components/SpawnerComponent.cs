using server.ECS;

namespace server.Simulation.Components
{
    [Component]
    public struct SpawnerComponent
    {
        public readonly int UnitIdToSpawn;

        // Spawn Interval 
        public readonly TimeSpan Interval;

        public float TimeSinceLastSpawn;

        // Amount of UnitType to spawn each Interval
        public readonly int AmountPerInterval;

        // Limit of living UnitIdToSpawn after which Interval will be ignored
        public readonly int MaxPopulation;

        // The idea is that when there's less than MinPopulation of living UnitIdToSpawn,
        // Interval will be ignored and units will be spawned each frame until 
        // MinPopulation of alive UnitIdToSpawn has been reached
        public readonly int MinPopulation;


        public SpawnerComponent(int unitId, TimeSpan interval, int amountPerInterval, int maxPopulation, int minPopulation)
        {
            UnitIdToSpawn = unitId;
            Interval = interval;
            AmountPerInterval = amountPerInterval;
            MaxPopulation = maxPopulation;
            MinPopulation = minPopulation;
            TimeSinceLastSpawn = 0;
        }
    }
}