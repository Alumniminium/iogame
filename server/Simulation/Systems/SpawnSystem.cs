using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

public sealed class SpawnSystem : NttSystem<Box2DBodyComponent, SpawnerComponent>
{
    public SpawnSystem() : base("Spawn System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent rigidBody, ref SpawnerComponent spawner)
    {
        spawner.TimeSinceLastSpawn += DeltaTime; // increment the timer

        var pop = SpawnManager.MapResources[spawner.UnitIdToSpawn]; // get current population

        if (pop >= spawner.MaxPopulation)
            return; // early return

        var vel = SpawnManager.GetRandomDirection() * 10; // random velocity pregen

        if (pop < spawner.MinPopulation) // spawn a single unit without checking the interval, also ignore spawn amount
        {
            SpawnManager.Spawn(Db.BaseResources[spawner.UnitIdToSpawn], rigidBody.Position, vel);
            return;
        }

        if (spawner.Interval.TotalMilliseconds > spawner.TimeSinceLastSpawn)
            return;

        spawner.TimeSinceLastSpawn = 0; // reset timer & do the spawning
        for (var x = 0; x < spawner.AmountPerInterval; x++)
            SpawnManager.Spawn(Db.BaseResources[spawner.UnitIdToSpawn], rigidBody.Position, vel);
    }
}