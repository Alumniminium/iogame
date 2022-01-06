using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 750;
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            if (phy.AngularVelocity == 0 && phy.Acceleration == Vector2.Zero && phy.Velocity == Vector2.Zero)
                return;

            Drag(ref phy);
            Velocity(ref phy);
            Position(in ntt, ref phy);

            if (phy.LastPosition == phy.Position)
                return;
            
            Replicate(in ntt, ref phy);
        }

        private static void Replicate(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            var phyRepl = new PhysicsReplicationComponent(ref phy);
            ntt.Replace(ref phyRepl);
        }

        private void Position(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            phy.RotationRadians += phy.AngularVelocity * deltaTime;

            phy.LastPosition = phy.Position;
            var newPosition = phy.Position + phy.Velocity * deltaTime;
            phy.Position = newPosition;


            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }

        private void Velocity(ref PhysicsComponent phy)
        {
            phy.Velocity += phy.Acceleration;
            // phy.Velocity += phy.Mass * new Vector2(0,0.2f) * deltaTime;
            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            if (phy.Velocity.Length() < 0.5 && phy.Acceleration.Length() < 0.5)
                phy.Velocity = Vector2.Zero;
            if (phy.AngularVelocity < 0.5)
                phy.AngularVelocity = 0f;
        }

        private static void Drag(ref PhysicsComponent phy)
        {
            phy.Velocity *= 1f - phy.Drag;
            phy.AngularVelocity *= 1f - phy.Drag;
        }
    }
}