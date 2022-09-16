using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public sealed class SpawnSystem : PixelSystem<PhysicsComponent, SpawnerComponent>
    {
        public SpawnSystem() : base("Spawn System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent c1, ref SpawnerComponent c2)
        {
            c2.TimeSinceLastSpawn += deltaTime * 1000; // increment the timer

            // var pop = SpawnManager.MapResources[c2.UnitIdToSpawn]; // get current population

            // if (pop >= c2.MaxPopulation)
            //     return; // early return

            var vel = SpawnManager.GetRandomDirection() * 10; // random velocity pregen

            // if (pop < c2.MinPopulation) // spawn a single unit without checking the interval, also ignore spawn amount
            // {
                // SpawnManager.Spawn(Db.BaseResources[c2.UnitIdToSpawn], c1.Position, vel);
            //     return;
            // }

            if (c2.Interval.TotalMilliseconds > c2.TimeSinceLastSpawn)
                return;

            c2.TimeSinceLastSpawn = 0; // reset timer & do the spawning
            for (var x = 0; x < c2.AmountPerInterval; x++)
                SpawnManager.Spawn(Db.BaseResources[c2.UnitIdToSpawn], c1.Position, vel);
        }
    }
}