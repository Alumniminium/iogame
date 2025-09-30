using System.Numerics;
using server.ECS;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// System that applies gravitational forces from gravity sources to all dynamic bodies within range.
/// </summary>
public sealed class GravitySystem : NttSystem<Box2DBodyComponent>
{
    public GravitySystem() : base("Gravity System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body)
    {
        if (!body.IsValid || body.IsStatic)
            return;

        var bodyPosition = body.Position;
        var totalForce = Vector2.Zero;

        foreach (var gravityNtt in NttQuery.Query<GravityComponent, Box2DBodyComponent>())
        {
            if (gravityNtt == ntt)
                continue;

            var gravity = gravityNtt.Get<GravityComponent>();
            var gravityBody = gravityNtt.Get<Box2DBodyComponent>();

            if (!gravityBody.IsValid)
                continue;

            var gravityPosition = gravityBody.Position;
            var direction = gravityPosition - bodyPosition;
            var distance = direction.Length();

            if (distance > gravity.Radius)
                continue;

            direction = Vector2.Normalize(direction);

            var force = direction * (gravity.Strength * body.Mass);

            totalForce += force;
        }

        if (totalForce.LengthSquared() > 0.01f)
        {
            body.ApplyForce(totalForce);
        }
    }
}