using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class DropSystem : PixelSystem<DeathTagComponent, PhysicsComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent ded, ref PhysicsComponent phy, ref DropResourceComponent pik)
        {
            if(pik.Amount == 0)
                return;
            SpawnManager.SpawnDrops(phy.Position, pik.Amount, Database.Db.BaseResources[pik.Id]);
        }
    }
}