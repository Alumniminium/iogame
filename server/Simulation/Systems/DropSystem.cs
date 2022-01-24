using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class DropSystem : PixelSystem<DeathTagComponent, PhysicsComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent ded, ref PhysicsComponent phy, ref DropResourceComponent pik)
        {
            if (pik.Amount == 0)
                return;

            var dropper = Db.BaseResources[pik.Id];
            var size = (int)Math.Max(1, dropper.Size / (pik.Amount * 0.5));

            for (var i = 0; i < pik.Amount; i++)
            {
                var lifetime = TimeSpan.FromSeconds(Random.Shared.Next(30, 120));
                var direction = SpawnManager.GetRandomDirection();
                var position = phy.Position + (direction * 2);
                SpawnManager.SpawnDrop(Db.BaseResources[Random.Shared.Next(3, dropper.Sides)], position, size, dropper.Color, lifetime, direction * 100);
            }
        }
    }
}