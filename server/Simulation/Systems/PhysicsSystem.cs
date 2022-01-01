using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 3750;
        public PhysicsSystem() : base("Physics System", Environment.ProcessorCount/2) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            phy.Rotation += phy.AngularVelocity * deltaTime;
            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;
            phy.AngularVelocity *= 1f - phy.Drag;

            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            var lastPosition = phy.Position;
            var newPosition = phy.Position + phy.Velocity * deltaTime;
            phy.Position = newPosition;

            if (lastPosition == phy.Position)
                return;

            if (phy.Velocity.Length() < 0.5 && phy.Acceleration.Length() < 0.5)
                phy.Velocity = Vector2.Zero;

            var phyRepl = new PhysicsReplicationComponent(ref phy);
            ntt.Set(ref phyRepl);

            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }
    }
}