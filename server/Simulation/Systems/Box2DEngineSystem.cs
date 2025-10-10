using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Simulation.Components;

namespace server.Simulation.Systems;

/// <summary>
/// Manages ship physics including thrust application, drag, rotational dampening, and energy consumption.
/// Applies forces from individual engine parts with selective firing for thrust vectoring and rotation control.
/// </summary>
public sealed class Box2DEngineSystem : NttSystem<Box2DBodyComponent, EngineComponent, EnergyComponent, InputComponent>
{
    public Box2DEngineSystem() : base("Box2D Engine System", threads: 1) { }

    public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref EnergyComponent nrg, ref InputComponent input)
    {
        if (!body.IsValid || body.IsStatic)
            return;

        HandleEnergyConsumption(ref eng, ref nrg);
        ApplyDrag(ref body, ref eng);
        ApplyRotationalDampening(ref body, ref eng);

        var isThrust = input.ButtonStates.HasFlag(PlayerInput.Thrust);
        var isBoost = input.ButtonStates.HasFlag(PlayerInput.Boost);
        var isLeft = input.ButtonStates.HasFlag(PlayerInput.Left);
        var isRight = input.ButtonStates.HasFlag(PlayerInput.Right);

        if (isThrust || isBoost || isLeft || isRight)
        {
            ApplyThrustFromEngineEntities(ntt, ref body, ref eng, input);
        }
    }

    /// <summary>
    /// Calculates and applies energy consumption from engine throttle, adjusting throttle if insufficient energy.
    /// </summary>
    private static void HandleEnergyConsumption(ref EngineComponent eng, ref EnergyComponent nrg)
    {
        var powerDraw = eng.PowerUse * eng.Throttle;
        if (nrg.AvailableCharge < powerDraw)
        {
            eng.Throttle = nrg.AvailableCharge / eng.PowerUse;
            powerDraw = eng.PowerUse * eng.Throttle;
            eng.ChangedTick = NttWorld.Tick;
        }
        nrg.DiscargeRateAcc += powerDraw;
    }

    /// <summary>
    /// Applies atmospheric drag to slow ship movement. Drag increases when RCS is active.
    /// </summary>
    private static void ApplyDrag(ref Box2DBodyComponent body, ref EngineComponent eng)
    {
        var dragCoeff = eng.RCS ? 0.01f : 0.005f;
        var dragForce = -body.LinearVelocity * dragCoeff;
        body.ApplyForce(dragForce);
    }

    /// <summary>
    /// Applies RCS dampening torque to reduce angular velocity when RCS is active.
    /// </summary>
    private static void ApplyRotationalDampening(ref Box2DBodyComponent body, ref EngineComponent eng)
    {
        if (eng.RCS && body.AngularVelocity != 0)
        {
            var rcsDampening = -body.AngularVelocity * 2f;
            body.ApplyTorque(rcsDampening);
        }
    }

    /// <summary>
    /// Iterates through all child entities with EngineComponent and applies thrust at their grid position/rotation.
    /// Uses ParentChildComponent grid coordinates to calculate world position and apply forces.
    /// Supports selective engine firing: A fires right-side engines, D fires left-side engines for rotation control.
    /// Works independently of W/Shift for pure rotational control.
    /// </summary>
    private static void ApplyThrustFromEngineEntities(in NTT parent, ref Box2DBodyComponent parentBody, ref EngineComponent parentEng, InputComponent input)
    {
        var isThrust = input.ButtonStates.HasFlag(PlayerInput.Thrust);
        var isBoost = input.ButtonStates.HasFlag(PlayerInput.Boost);
        var isLeft = input.ButtonStates.HasFlag(PlayerInput.Left);
        var isRight = input.ButtonStates.HasFlag(PlayerInput.Right);

        var thrustMultiplier = isBoost ? 1f : (isThrust ? parentEng.Throttle : 0f);

        foreach (var childEntity in NttQuery.Query<ParentChildComponent, EngineComponent>())
        {
            ref readonly var parentChild = ref childEntity.Get<ParentChildComponent>();
            if (parentChild.ParentId != parent)
                continue;

            ref readonly var engineComp = ref childEntity.Get<EngineComponent>();

            // Calculate world position from grid coordinates
            var localOffset = new Vector2(parentChild.GridX, parentChild.GridY);
            var rotatedOffset = Vector2.Transform(localOffset, Matrix3x2.CreateRotation(parentBody.Rotation));
            var worldPosition = parentBody.Position + rotatedOffset;

            // Calculate absolute rotation from parent rotation + part rotation (0-3 -> radians)
            var partRotationRad = parentChild.Rotation * MathF.PI / 2f;
            var absoluteRotation = parentBody.Rotation + partRotationRad;
            var thrustDirection = new Vector2(MathF.Cos(absoluteRotation), MathF.Sin(absoluteRotation));

            // Determine if this engine should fire for rotation control
            // A (Left) -> fire bottom engines (GridY > 0) -> counter-clockwise rotation
            // D (Right) -> fire top engines (GridY < 0) -> clockwise rotation
            var shouldFireForRotation = false;
            if (isLeft && parentChild.GridY > 0)
                shouldFireForRotation = true;
            if (isRight && parentChild.GridY < 0)
                shouldFireForRotation = true;

            // Calculate total thrust: forward thrust + rotation thrust
            var totalThrust = 0f;
            if (thrustMultiplier > 0)
                totalThrust += engineComp.MaxThrustNewtons * thrustMultiplier;
            if (shouldFireForRotation)
                totalThrust += engineComp.MaxThrustNewtons;

            // Apply combined thrust force
            if (totalThrust > 0)
            {
                var thrustForce = thrustDirection * totalThrust;
                parentBody.ApplyForce(thrustForce, worldPosition);
            }
        }
    }

}
