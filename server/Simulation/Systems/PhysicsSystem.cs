using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.Caching.Distributed;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 500;
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            if (phy.AngularVelocity == 0 && phy.Acceleration == Vector2.Zero && phy.Velocity == Vector2.Zero)
                return;

            if (float.IsNaN(phy.Velocity.X))
                phy.Velocity = Vector2.Zero;

            var dist = Vector2.Distance(phy.Position, new Vector2(Game.MapSize.X/2,Game.MapSize.Y));
            if(dist < 1000)
            {
                var force = 10000 / dist;
                phy.Velocity += new Vector2(0, force) * deltaTime;
            }
            dist = Vector2.Distance(phy.Position, new Vector2(Game.MapSize.X/2,0));
            if(dist < 1000)
            {
                var force = 10000 / dist;
                phy.Velocity += new Vector2(0, -force) * deltaTime;
            }
            phy.AngularVelocity *= 1f - phy.Drag;
            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;

            // phy.Velocity += phy.Mass * new Vector2(0,0.2f) * deltaTime;
            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            phy.LastPosition = phy.Position;

            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            var newPosition = phy.Position + phy.Velocity * deltaTime;
            
            phy.Position = newPosition;

            if (phy.LastPosition == phy.Position)
                return;

            if (phy.Velocity.Length() < 0.5 && phy.Acceleration.Length() < 0.5)
                phy.Velocity = Vector2.Zero;
            if (phy.AngularVelocity < 0.5)
                phy.AngularVelocity = 0f;

            var phyRepl = new PhysicsReplicationComponent(ref phy);
            ntt.Replace(ref phyRepl);
            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }
    }
}