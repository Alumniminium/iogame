
using System;
using System.Linq;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Net;

namespace server.Simulation.Systems
{
    public sealed class Box2DEngineSystem : NttSystem<Box2DBodyComponent, EngineComponent, EnergyComponent, ShipConfigurationComponent, InputComponent>
    {
        public Box2DEngineSystem() : base("Box2D Engine System", threads: 1) { }

        public override void Update(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref EnergyComponent nrg, ref ShipConfigurationComponent shipConfig, ref InputComponent input)
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
                HandleThrustAndExhaust(ntt, ref body, ref eng, ref shipConfig, input);
            }
        }

        private void HandleEnergyConsumption(ref EngineComponent eng, ref EnergyComponent nrg)
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

        private void ApplyDrag(ref Box2DBodyComponent body, ref EngineComponent eng)
        {
            var dragCoeff = eng.RCS ? 0.01f : 0.005f;
            var dragForce = -body.LinearVelocity * dragCoeff;
            body.ApplyForce(dragForce);
        }

        private void ApplyRotationalDampening(ref Box2DBodyComponent body, ref EngineComponent eng)
        {
            if (eng.RCS && body.AngularVelocity != 0)
            {
                var rcsDampening = -body.AngularVelocity * 2f;
                body.ApplyTorque(rcsDampening);
            }
        }

        private void HandleThrustAndExhaust(in NTT ntt, ref Box2DBodyComponent body, ref EngineComponent eng, ref ShipConfigurationComponent shipConfig, InputComponent input)
        {
            var engineParts = shipConfig.Parts.Where(p => p.Type == 2).ToList();
            var isThrust = input.ButtonStates.HasFlag(PlayerInput.Thrust);
            var isBoost = input.ButtonStates.HasFlag(PlayerInput.Boost);
            var isLeft = input.ButtonStates.HasFlag(PlayerInput.Left);
            var isRight = input.ButtonStates.HasFlag(PlayerInput.Right);

            var thrustMultiplier = isBoost ? 1f : (isThrust ? eng.Throttle : 0.7f);
            var thrustPerEngine = eng.MaxThrustNewtons * thrustMultiplier;

            var bodyRotationMatrix = Matrix3x2.CreateRotation(body.Rotation);

            foreach (var enginePart in engineParts)
            {
                var gridOffsetX = enginePart.GridX - shipConfig.CenterX;
                var gridOffsetY = enginePart.GridY - shipConfig.CenterY;

                DetermineFireState(isThrust, isBoost, isLeft, isRight, gridOffsetY, out var shouldFire, out var thrustReduction);

                if (!shouldFire)
                    continue;

                var gridOffset = new Vector2(gridOffsetX, gridOffsetY);
                var localPos = gridOffset;
                var engineWorldPos = body.Position + Vector2.Transform(localPos, bodyRotationMatrix);

                var engineRotationRad = enginePart.Rotation * MathF.PI / 2f;
                var totalRotation = body.Rotation + engineRotationRad;
                var thrustDirection = new Vector2(MathF.Cos(totalRotation), MathF.Sin(totalRotation));

                var thrustForce = thrustDirection * (thrustPerEngine * thrustReduction);
                body.ApplyForce(thrustForce, engineWorldPos);

                HandleSingleExhaust(ntt, ref eng, engineWorldPos, totalRotation, thrustPerEngine);
            }
        }

        private void DetermineFireState(bool isThrust, bool isBoost, bool isLeft, bool isRight, float offsetY, out bool shouldFire, out float thrustReduction)
        {
            shouldFire = false;
            thrustReduction = 1.0f;

            if (isThrust || isBoost)
            {
                shouldFire = true;
                if (isLeft && offsetY < 0)
                    thrustReduction = 0.7f;
                else if (isRight && offsetY > 0)
                    thrustReduction = 0.7f;
            }
            else if (isLeft || isRight)
            {
                // Allow both left and right to work simultaneously
                shouldFire = (isLeft && offsetY > 0) || (isRight && offsetY < 0);
            }
        }

        private void HandleSingleExhaust(in NTT ntt, ref EngineComponent eng, Vector2 engineWorldPos, float totalRotation, float thrustPerEngine)
        {
            var exhaustDir = new Vector2(-MathF.Cos(totalRotation), -MathF.Sin(totalRotation));
            var direction = exhaustDir.ToRadians();
            var deg = direction.ToDegrees();

            var ray = new Ray(engineWorldPos, deg + (5 * Random.Shared.Next(-6, 7)));
            ref readonly var vwp = ref ntt.Get<ViewportComponent>();

            foreach (var entity in vwp.EntitiesVisible)
            {
                if (!entity.Has<Box2DBodyComponent>())
                    continue;

                ref var bBody = ref entity.Get<Box2DBodyComponent>();
                if (ray.Cast(bBody.Position, 10f) is var rayHit && rayHit != Vector2.Zero)
                {
                    if (eng.ChangedTick < NttWorld.Tick)
                    {
                        ntt.NetSync(RayPacket.Create(ntt, entity, engineWorldPos, rayHit));
                        eng.ChangedTick = NttWorld.Tick;
                    }
                }
            }
        }
    }
}
