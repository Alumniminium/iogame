using System;
using System.Numerics;
using server.ECS;
using server.Simulation.Components;
using server.Simulation.Database;
using server.Simulation.Managers;

namespace server.Simulation.Systems;

public sealed class DropSystem : NttSystem<DeathTagComponent, PhysicsComponent, DropResourceComponent>
{
    public DropSystem() : base("Drop System", threads: 1) { }

    public override void Update(in NTT ntt, ref DeathTagComponent dtc, ref PhysicsComponent rigidBody, ref DropResourceComponent pik)
    {
        if (pik.Amount == 0)
            return;
        // get random Db.BaseResource
        var randomId = Random.Shared.Next(3, 7);
        var resource = Db.BaseResources[randomId];
        var killerPos = dtc.Killer.Get<PhysicsComponent>().Position;

        for (var i = 0; i < pik.Amount; i++)
        {
            var lifetime = TimeSpan.FromMilliseconds(Random.Shared.Next(5000, 10000));
            var direction = Vector2.Normalize(killerPos - rigidBody.Position);
            var position = rigidBody.Position + (direction * 2);
            SpawnManager.SpawnDrop(Db.BaseResources[Random.Shared.Next(3, 8)], position, lifetime, direction * 10f);
        }
    }
}