using System;
using System.Numerics;
using server.ECS;
using server.Enums;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems;

public sealed class PhysicsSystem : NttSystem<PhysicsComponent>
{
    public const int SpeedLimit = 400;
    public const int PhysicsSubsteps = 8;
    public PhysicsSystem() : base("Physics System", threads: 1) { }
    protected override bool MatchesFilter(in NTT nttId) => !nttId.Get<PhysicsComponent>().Static && base.MatchesFilter(nttId);

    public override void Update(in NTT a, ref PhysicsComponent phy)
    {
        if (phy.Acceleration == Vector2.Zero && phy.LinearVelocity == Vector2.Zero && phy.AngularVelocity == 0 && phy.Position == phy.LastPosition)
            return;

        var size = phy.ShapeType == ShapeType.Circle ? new Vector2(phy.Radius) : new Vector2(phy.Width, phy.Height);

        phy.LastPosition = phy.Position;
        phy.LastRotation = phy.RotationRadians;

        // Apply gravity
        if (phy.Position.Y > Game.MapSize.Y - 1500)
            phy.Acceleration += new Vector2(0, 9.81f) * DeltaTime;

        if (float.IsNaN(phy.Acceleration.X) || float.IsNaN(phy.Acceleration.Y))
            phy.Acceleration = Vector2.Zero;

        // Update rotation
        phy.RotationRadians += phy.AngularVelocity * DeltaTime;
        phy.AngularVelocity *= 1f - (phy.Drag * DeltaTime);

        if (phy.RotationRadians > MathF.PI * 2)
            phy.RotationRadians -= MathF.PI * 2;
        if (phy.RotationRadians < 0)
            phy.RotationRadians += MathF.PI * 2;

        if (MathF.Abs(phy.AngularVelocity) < 0.1)
            phy.AngularVelocity = 0;

        // Update velocity
        phy.LinearVelocity += phy.Acceleration;
        phy.LinearVelocity = phy.LinearVelocity.ClampMagnitude(SpeedLimit);
        phy.LinearVelocity *= 1f - (phy.Drag * DeltaTime);
        phy.Acceleration = Vector2.Zero;

        if (float.IsNaN(phy.LinearVelocity.X) || float.IsNaN(phy.LinearVelocity.Y))
            phy.LinearVelocity = Vector2.Zero;

        if (phy.LinearVelocity.Length() < 0.1)
            phy.LinearVelocity = Vector2.Zero;

        // Update position
        var newPosition = phy.Position + (phy.LinearVelocity * DeltaTime);
        newPosition = Vector2.Clamp(newPosition, size, Game.MapSize - size);
        phy.Position = newPosition;

        // Handle boundary collisions
        if (phy.Position.X == size.X || phy.Position.X == Game.MapSize.X - size.X)
            phy.LinearVelocity.X = -phy.LinearVelocity.X * phy.Elasticity;

        if (phy.Position.Y == size.Y || phy.Position.Y == Game.MapSize.Y - size.Y)
        {
            phy.LinearVelocity.Y = -phy.LinearVelocity.Y * phy.Elasticity;
            if (!a.Has<NetworkComponent>())
            {
                var dtc = new DeathTagComponent(a, default);
                a.Set(ref dtc);
            }
        }

        if (phy.Position != phy.LastPosition || phy.RotationRadians != phy.LastRotation)
        {
            phy.TransformUpdateRequired = true;
            phy.AABBUpdateRequired = true;
            phy.ChangedTick = NttWorld.Tick;
            if (phy.Position != phy.LastPosition)
                Game.Grid.Move(in a, ref phy);
        }
    }
}