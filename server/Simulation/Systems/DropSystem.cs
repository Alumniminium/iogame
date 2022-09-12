using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public class DropSystem : PixelSystem<DeathTagComponent, PhysicsComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent c1, ref PhysicsComponent c2, ref DropResourceComponent c3)
        {
            if (c3.Amount == 0)
                return;

            var dropper = Db.BaseResources[c3.Id];
            var size = (int)MathF.Max(1, dropper.Size / (c3.Amount * 0.5f));

            for (var i = 0; i < c3.Amount; i++)
            {
                var lifetime = TimeSpan.FromSeconds(Random.Shared.Next(30, 120));
                var direction = SpawnManager.GetRandomDirection();
                var position = c2.Position + (direction * 2);
                SpawnManager.SpawnDrop(Db.BaseResources[Random.Shared.Next(3, dropper.Sides)], position, size, dropper.Color, lifetime, direction * 100);
            }
        }
    }
}