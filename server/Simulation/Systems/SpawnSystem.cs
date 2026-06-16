using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

/// <summary>
/// Manages resource spawners that generate pickups (triangles, squares, pentagons) at regular intervals.
/// Maintains population limits and ensures minimum populations are met immediately.
/// </summary>
public sealed class SpawnSystem : NttSystem<PhysicsComponent, SpawnerComponent>
{
    public SpawnSystem() : base("Spawn System", threads: 1) { }

    public override void Update(in NTT ntt, ref PhysicsComponent rigidBody, ref SpawnerComponent spawner)
    {
        spawner.TimeSinceLastSpawn += DeltaTime;

        var pop = SpawnManager.MapResources[spawner.UnitIdToSpawn];

        if (pop >= spawner.MaxPopulation)
            return;

        var vel = SpawnManager.GetRandomDirection() * 10;

        if (pop < spawner.MinPopulation)
        {
            SpawnManager.Spawn(Db.BaseResources[spawner.UnitIdToSpawn], rigidBody.Position, vel);
            return;
        }

        if (spawner.Interval.TotalMilliseconds > spawner.TimeSinceLastSpawn)
            return;

        spawner.TimeSinceLastSpawn = 0;
        for (var x = 0; x < spawner.AmountPerInterval; x++)
            SpawnManager.Spawn(Db.BaseResources[spawner.UnitIdToSpawn], rigidBody.Position, vel);
    }
}