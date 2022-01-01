using System.Numerics;
using server.ECS;

namespace server.Simulation.Components.Replication
{
    [Component]
    public struct PhysicsReplicationComponent
    {
        public uint CreatedTick;
        public float Mass;
        public float Elasticity;
        public float Drag;
        public float Rotation;
        public float AngularVelocity;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 Velocity;

        public PhysicsReplicationComponent(ref PhysicsComponent phy)
        {
            CreatedTick = Game.CurrentTick;
            AngularVelocity= phy.AngularVelocity;
            Mass = phy.Mass;
            Elasticity = phy.Elasticity;
            Drag = phy.Drag;
            Acceleration = phy.Acceleration;
            Velocity = phy.Velocity;
            Position = phy.Position;
            Rotation = phy.Rotation;
        }
    }
}