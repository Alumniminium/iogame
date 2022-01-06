using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class SpawnSystem : PixelSystem<PhysicsComponent, SpawnerComponent>
    {
        public SpawnSystem() : base("Spawn System", threads: 1) { }

        /// The Spawn System.    
        ///        Written by George R. R. Martin
        /// Keeps Resources on the Map over time.
        /// This system has three jobs
        ///
        /// 1. Keep the map populated at all times.
        /// 2. Keep the population limited to not overwhelm the hardware
        /// 3. Deal with players farming the map empty before spawn interval fires
        /// 
        /// The SpawnerComponent was created to provide a Min and Max population
        /// which is important to consider. We never want a simulationframe with 
        /// zero resources on the map. Since the spawn rate is on a timer interval,
        /// there's a chance the players could clean upthe map. So, each frame
        /// we spawn a single unit until we reach the min population before we even
        /// start paying any attention to the interval.

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref SpawnerComponent spwn)
        {
            spwn.TimeSinceLastSpawn += deltaTime; // increment the timer

            var pop = SpawnManager.MapResources[spwn.UnitIdToSpawn]; // get current population

            if (pop >= spwn.MaxPopulation)
                return; // early return

            var vel = SpawnManager.GetRandomVelocity() * 10; // random velocity pregen

            if (pop < spwn.MinPopulation) // spawn a single unit without checking the interval, also ignore spawn amount
            {
                SpawnManager.Spawn(Db.BaseResources[spwn.UnitIdToSpawn], phy.Position, vel);
                return;
            }

            if (spwn.Interval.TotalMilliseconds > spwn.TimeSinceLastSpawn)
                return;

            spwn.TimeSinceLastSpawn = 0; // reset timer & do the spawning
            for (int x = 0; x < spwn.AmountPerInterval; x++)
                SpawnManager.Spawn(Db.BaseResources[spwn.UnitIdToSpawn], phy.Position, vel);
        }
    }
}