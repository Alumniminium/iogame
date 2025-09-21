using System.Collections.Generic;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public class ShipPropulsionSystem : NttSystem<AssemblyComponent>
{
    public ShipPropulsionSystem() : base("Ship Propulsion", threads: 1) { }
    public override void Update(in NTT ship, ref AssemblyComponent assembly)
    {
        if (!assembly.IsMobile) return;

        Vector2 totalThrust = Vector2.Zero;
        float totalTorque = 0f;

        foreach (var block in GetBlocksOfAssembly(ship))
        {
            if (!block.Has<DirectionalEngineComponent>()) continue;

            var engine = block.Get<DirectionalEngineComponent>();
            var gridPos = block.Get<GridPositionComponent>();

            // Engine thrust direction based on block rotation
            var thrustDirection = gridPos.Rotation.GetDirection();
            var thrustForce = thrustDirection * engine.Thrust;

            totalThrust += thrustForce;

            // Calculate torque for off-center engines
            var blockWorldPos = GridToWorld(gridPos.GridPos, ship);
            var leverArm = blockWorldPos - ship.Get<Box2DBodyComponent>().Position;
            var torque = leverArm.Cross(thrustForce);
            totalTorque += torque;
        }

        // Apply combined forces to ship
        if (ship.Has<Box2DBodyComponent>())
        {
            ref var body = ref ship.Get<Box2DBodyComponent>();
            body.ApplyForce(totalThrust);
            body.ApplyTorque(totalTorque);
        }
    }

    private static IEnumerable<NTT> GetBlocksOfAssembly(NTT ship)
    {
        // This would need to query all entities with GridPositionComponent
        // where Assembly == ship
        // For now, return empty collection
        yield break;
    }

    private static Vector2 GridToWorld(Vector2Int gridPos, NTT ship)
    {
        // Convert grid position to world position relative to ship
        // This is a simplified implementation
        if (ship.Has<Box2DBodyComponent>())
        {
            var shipPos = ship.Get<Box2DBodyComponent>().Position;
            return shipPos + new Vector2(gridPos.X, gridPos.Y);
        }
        return new Vector2(gridPos.X, gridPos.Y);
    }
}