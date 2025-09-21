using System;
using System.Drawing;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

public sealed class Box2DEngineSystem : NttSystem<Box2DBodyComponent, EngineComponent, EnergyComponent>
{
    public Box2DEngineSystem() : base("Box2D Engine System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref EnergyComponent nrg)
    {
        if (!body.IsValid || body.IsStatic)
            return;

        var powerDraw = eng.PowerUse * eng.Throttle;

        if (nrg.AvailableCharge < powerDraw)
        {
            eng.Throttle = nrg.AvailableCharge / eng.PowerUse;
            powerDraw = eng.PowerUse * eng.Throttle;
            eng.ChangedTick = NttWorld.Tick;
        }

        nrg.DiscargeRateAcc += powerDraw;

        // Sync from Box2D to get current physics state
        body.SyncFromBox2D();

        // Apply drag force based on RCS state
        var dragCoeff = eng.RCS ? 0.01f : 0.005f;
        var dragForce = -body.LinearVelocity * dragCoeff;
        body.ApplyForce(dragForce);

        // Apply rotation torque to turn the body
        if (eng.Rotation != 0)
        {
            // Use appropriate torque for the test box
            var thrusterTorque = eng.RCS ? 1f : 5f; // N⋅m (small torque for 1kg box)
            var torque = eng.Rotation * thrusterTorque;
            body.ApplyTorque(torque);
        }

        // Apply rotational dampening when RCS is on
        if (eng.RCS && body.AngularVelocity != 0)
        {
            var rcsDampening = -body.AngularVelocity * 2f; // Appropriate dampening for 1kg box
            body.ApplyTorque(rcsDampening);
        }

        // Calculate forward direction (where the nose points)
        // Force is applied in the direction the rocket is pointing
        var forwardDir = new Vector2(MathF.Cos(body.Rotation), MathF.Sin(body.Rotation));
        // Use thrust directly in Newtons
        var propulsionForce = forwardDir * (eng.MaxThrustNewtons * eng.Throttle);

        // Apply propulsion force
        if (eng.Throttle > 0)
        {
            FConsole.WriteLine($"🔥 Thrust: {propulsionForce} N, Direction: {forwardDir}, Throttle: {eng.Throttle}");
            FConsole.WriteLine($"📊 Velocity: {body.LinearVelocity} m/s, Position: {body.Position}");
            body.ApplyForce(propulsionForce);
        }

        if (eng.Throttle == 0 && eng.Rotation == 0 && !eng.RCS)
            return;

        if (eng.Throttle == 0)
            return;

        // Raycast effects for engine exhaust (opposite to forward direction)
        var exhaustDir = -forwardDir;
        var direction = exhaustDir.ToRadians();
        var deg = direction.ToDegrees();

        var ray = new Ray(body.Position, deg + (5 * Random.Shared.Next(-6, 7)));
        ref readonly var vwp = ref ntt.Get<ViewportComponent>();
        for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
        {
            var b = vwp.EntitiesVisible[i];
            if (!b.Has<Box2DBodyComponent>())
                continue;

            ref var bBody = ref b.Get<Box2DBodyComponent>();
            Vector2 rayHit = default;

            // For now, assume circular hit detection - Box2D shapes are more complex
            // This could be improved by querying Box2D shape data
            var radius = 10f; // Approximate radius for hit detection
            rayHit = ray.Cast(bBody.Position, radius);

            if (rayHit != Vector2.Zero)
            {
                var effectForce = exhaustDir * (eng.MaxThrustNewtons * eng.Throttle * 0.1f);
                var distance = Vector2.Distance(body.Position, bBody.Position);
                var falloff = MathF.Max(0.1f, 1f - (distance / 100f));
                var finalForce = effectForce * falloff;

                if (b.Has<Box2DBodyComponent>())
                {
                    ref var targetBody = ref b.Get<Box2DBodyComponent>();
                    targetBody.ApplyForce(finalForce, rayHit);
                }

                if (eng.ChangedTick < NttWorld.Tick)
                {
                    ntt.NetSync(RayPacket.Create(ntt, b, body.Position, rayHit));
                    eng.ChangedTick = NttWorld.Tick;
                }
            }
        }
    }
}