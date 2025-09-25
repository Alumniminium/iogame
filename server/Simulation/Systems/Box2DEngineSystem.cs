using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems;

public sealed class Box2DEngineSystem : NttSystem<Box2DBodyComponent, EngineComponent, EnergyComponent, ShipConfigurationComponent, InputComponent>
{
    public Box2DEngineSystem() : base("Box2D Engine System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref EnergyComponent nrg, ref ShipConfigurationComponent shipConfig, ref InputComponent input)
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

        // No sync needed - properties directly access Box2D data

        // Apply drag force based on RCS state
        var dragCoeff = eng.RCS ? 0.01f : 0.005f;
        var dragForce = -body.LinearVelocity * dragCoeff;
        body.ApplyForce(dragForce);

        // Rotation now handled by selective engine firing instead of direct torque

        // Apply rotational dampening when RCS is on
        if (eng.RCS && body.AngularVelocity != 0)
        {
            var rcsDampening = -body.AngularVelocity * 2f; // Appropriate dampening for 1kg box
            body.ApplyTorque(rcsDampening);
        }

        // Apply selective thrust from positioned engines based on inputs
        var isThrust = input.ButtonStates.HasFlag(PlayerInput.Thrust);
        var isBoost = input.ButtonStates.HasFlag(PlayerInput.Boost);
        var isLeft = input.ButtonStates.HasFlag(PlayerInput.Left);
        var isRight = input.ButtonStates.HasFlag(PlayerInput.Right);

        if (isThrust || isBoost || isLeft || isRight)
        {
            var engineParts = shipConfig.Parts.Where(p => p.Type == 2).ToList(); // Engine parts only

            if (engineParts.Count > 0)
            {
                var baseEngineThrust = 35f; // Base thrust per engine part
                var thrustMultiplier = isBoost ? 1f : (isThrust ? eng.Throttle : 0.7f); // Boost=full, normal thrust=throttle, maneuvering=reduced
                var thrustPerEngine = baseEngineThrust * thrustMultiplier;

                foreach (var enginePart in engineParts)
                {
                    // Convert grid coordinates to world offset (relative to center)
                    var offsetX = enginePart.GridX - shipConfig.CenterX;
                    var offsetY = enginePart.GridY - shipConfig.CenterY;


                    // Determine if this engine should fire based on input and engine position
                    bool shouldFire = false;

                    if (isThrust || isBoost)
                    {
                        // Shift fires all engines
                        shouldFire = true;
                    }
                    else if (isLeft)
                    {
                        // A fires bottom engines (to push ship left)
                        shouldFire = offsetY > 0;
                    }
                    else if (isRight)
                    {
                        // D fires top engines (to push ship right)
                        shouldFire = offsetY < 0;
                    }

                    if (!shouldFire) continue;

                    // Calculate engine world position
                    var cos = MathF.Cos(body.Rotation);
                    var sin = MathF.Sin(body.Rotation);
                    var engineWorldPos = new Vector2(
                        body.Position.X + offsetX * cos - offsetY * sin,
                        body.Position.Y + offsetX * sin + offsetY * cos
                    );

                    // Calculate engine thrust direction (engine rotation + ship rotation)
                    var engineRotationRad = enginePart.Rotation * MathF.PI / 2f;
                    var thrustDirection = body.Rotation + engineRotationRad;
                    var thrustVector = new Vector2(MathF.Cos(thrustDirection), MathF.Sin(thrustDirection));

                    // Apply thrust force at engine position
                    var thrustForce = thrustVector * thrustPerEngine;
                    body.ApplyForce(thrustForce, engineWorldPos);
                }
            }
            else
            {
                // Fallback to center thrust if no engine parts configured
                if (isThrust || isBoost)
                {
                    var forwardDir = new Vector2(MathF.Cos(body.Rotation), MathF.Sin(body.Rotation));
                    var thrustMultiplier = isBoost ? 1f : eng.Throttle;
                    var propulsionForce = forwardDir * (eng.MaxThrustNewtons * thrustMultiplier);
                    body.ApplyForce(propulsionForce);
                }
            }
        }

        if (eng.Rotation == 0 && !eng.RCS && !isThrust && !isBoost && !isLeft && !isRight)
            return;

        // Raycast effects for engine exhaust from positioned engines (only for actively firing engines)
        if (isThrust || isBoost || isLeft || isRight)
        {
            var engineParts = shipConfig.Parts.Where(p => p.Type == 2).ToList();

            foreach (var enginePart in engineParts)
            {
                // Convert grid coordinates to world offset (relative to center)
                var offsetX = enginePart.GridX - shipConfig.CenterX;
                var offsetY = enginePart.GridY - shipConfig.CenterY;

                // Check if this engine should fire (same logic as thrust application)
                bool shouldFire = false;
                if (isThrust || isBoost)
                {
                    // Shift fires all engines
                    shouldFire = true;
                }
                else if (isLeft)
                {
                    // A fires bottom engines
                    shouldFire = offsetY > 0;
                }
                else if (isRight)
                {
                    // D fires top engines
                    shouldFire = offsetY < 0;
                }

                if (!shouldFire) continue;

                // Calculate engine world position
                var cos = MathF.Cos(body.Rotation);
                var sin = MathF.Sin(body.Rotation);
                var engineWorldPos = new Vector2(
                    body.Position.X + offsetX * cos - offsetY * sin,
                    body.Position.Y + offsetX * sin + offsetY * cos
                );

                // Calculate exhaust direction (opposite to engine thrust direction)
                var engineRotationRad = enginePart.Rotation * MathF.PI / 2f;
                var thrustDirection = body.Rotation + engineRotationRad;
                var exhaustDir = new Vector2(-MathF.Cos(thrustDirection), -MathF.Sin(thrustDirection));

                var direction = exhaustDir.ToRadians();
                var deg = direction.ToDegrees();

                var ray = new Ray(engineWorldPos, deg + (5 * Random.Shared.Next(-6, 7)));
                ref readonly var vwp = ref ntt.Get<ViewportComponent>();

                for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
                {
                    var b = vwp.EntitiesVisible[i];
                    if (!b.Has<Box2DBodyComponent>())
                        continue;

                    ref var bBody = ref b.Get<Box2DBodyComponent>();
                    Vector2 rayHit = default;

                    var radius = 10f;
                    rayHit = ray.Cast(bBody.Position, radius);

                    if (rayHit != Vector2.Zero)
                    {
                        var baseEngineThrust = 35f;
                        var thrustMultiplier = isBoost ? 1f : (isThrust ? eng.Throttle : 0.7f);
                        var thrustPerEngine = baseEngineThrust * thrustMultiplier;
                        var effectForce = exhaustDir * (thrustPerEngine * 0.1f);
                        var distance = Vector2.Distance(engineWorldPos, bBody.Position);
                        var falloff = MathF.Max(0.1f, 1f - (distance / 100f));
                        var finalForce = effectForce * falloff;

                        if (b.Has<Box2DBodyComponent>())
                        {
                            ref var targetBody = ref b.Get<Box2DBodyComponent>();
                            targetBody.ApplyForce(finalForce, rayHit);
                        }

                        if (eng.ChangedTick < NttWorld.Tick)
                        {
                            ntt.NetSync(RayPacket.Create(ntt, b, engineWorldPos, rayHit));
                            eng.ChangedTick = NttWorld.Tick;
                        }
                    }
                }
            }
        }
    }
}