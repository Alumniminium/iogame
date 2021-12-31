using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 3750;
        public PhysicsSystem() : base("Physics System", Environment.ProcessorCount/2) { }

        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            // if (phy.Velocity == Vector2.Zero && phy.Acceleration == Vector2.Zero)
            //     return;

            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;

            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            // if (phy.Velocity.Length() < 1 && phy.Acceleration == Vector2.Zero)
            //     phy.Velocity = Vector2.Zero;

            phy.LastPosition = phy.Position;
            var newPosition = phy.Position + phy.Velocity * deltaTime;
            phy.Position = newPosition;
        }
    }
}