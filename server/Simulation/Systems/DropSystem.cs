using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class DropSystem : PixelSystem<DeathTagComponent, PhysicsComponent, PickupComponent>
    {
        public DropSystem() : base("Drop System", threads: Environment.ProcessorCount) { }


        public override void Update(in PixelEntity ntt, ref DeathTagComponent ded, ref PhysicsComponent phy, ref PickupComponent pik)
        {
            for (int x = 0; x < pik.Amount; x++)
                SpawnManager.Spawn(Database.Db.BaseResources[0], phy.Position, Vector2.Zero);
        }
    }
}