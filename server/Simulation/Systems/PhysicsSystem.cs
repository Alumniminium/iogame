using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent, ShapeComponent>
    {
        public const int SpeedLimit = 500;
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy, ref ShapeComponent shp)
        {
            if (phy.AngularVelocity == 0 && phy.Acceleration == Vector2.Zero && phy.Velocity == Vector2.Zero)
                return;

            if (float.IsNaN(phy.Velocity.X))
                phy.Velocity = Vector2.Zero;

            ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2,Game.MapSize.Y), 1000);
            ApplyGravity(ref phy, new Vector2(Game.MapSize.X / 2,0), 2000);
            
            phy.AngularVelocity *= 1f - phy.Drag;
            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;

            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            phy.LastPosition = phy.Position;

            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            var newPosition = phy.Position + phy.Velocity * deltaTime;

            var size = new Vector2(shp.Radius, shp.Radius);
            newPosition = Vector2.Clamp(newPosition, size, Game.MapSize-size);

            phy.Position = newPosition;

            if (phy.LastPosition == phy.Position)
                return;

            if (phy.Velocity.Length() < 0.01 && phy.Acceleration.Length() < 0.01)
                phy.Velocity = Vector2.Zero;

            if(phy.Position.X == size.X || phy.Position.X == Game.MapSize.X - size.X)
            {
                phy.Velocity.X = -phy.Velocity.X * phy.Elasticity;
            }
            if(phy.Position.Y == size.Y || phy.Position.Y == Game.MapSize.Y - size.Y)
            {
                phy.AngularVelocity *= 0.99f;
                phy.Velocity.Y = -phy.Velocity.Y * phy.Elasticity;
                phy.Velocity.X *= 0.99f;
            }

            if (phy.AngularVelocity < 0.5)
                phy.AngularVelocity = 0f;

            var phyRepl = new PhysicsReplicationComponent(ref phy);
            ntt.Replace(ref phyRepl);
            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }

        private void ApplyGravity(ref PhysicsComponent phy, Vector2 gravityOrigin, float maxDistance)
        {
            var dist = Vector2.Distance(phy.Position, gravityOrigin);
            if (dist >= maxDistance)
                return;

            var dir = Vector2.Normalize(gravityOrigin - Vector2.Normalize(phy.Position));

            var force = phy.Mass / 100;
            if (dir.Y < 0)
                phy.Velocity += new Vector2(0, -force) * deltaTime;
            else
                phy.Velocity += new Vector2(0, force) * deltaTime;
        }
    }
}