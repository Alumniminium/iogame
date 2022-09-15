using System;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems
{
    public sealed class DropSystem : PixelSystem<DeathTagComponent, PhysicsComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent c1, ref PhysicsComponent c2, ref DropResourceComponent c3)
        {
            var dropper = Db.BaseResources[c3.Id];
            var size = (int)MathF.Max(1, dropper.Size / c3.Amount);

            for (var i = 0; i < c3.Amount; i++)
            {
                var lifetime = TimeSpan.FromMilliseconds(Random.Shared.Next(5000, 10000));
                var direction = SpawnManager.GetRandomDirection();
                var position = c2.Position + (direction *2);
                SpawnManager.SpawnDrop(Db.BaseResources[Random.Shared.Next(3, 8)], position, size, dropper.Color, lifetime, direction * 100);
            }
        }
    }
}