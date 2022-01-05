using System.Collections.Concurrent;
using System.Numerics;
using server.ECS;
using server.Helpers;
using server.Simulation.Components;
using server.Simulation.Components.Replication;
using server.Simulation.Entities;

namespace server.Simulation.Systems
{
    public class PhysicsSystem : PixelSystem<PhysicsComponent>
    {
        public const int SpeedLimit = 750;
        public readonly ConcurrentStack<ShapeEntity> MovedEntitiesThisFrame = new();
        public PhysicsSystem() : base("Physics System", threads: Environment.ProcessorCount) { }

        protected override void PreUpdate() => MovedEntitiesThisFrame.Clear();
        public override void Update(in PixelEntity ntt, ref PhysicsComponent phy)
        {
            if(phy.AngularVelocity == 0 && phy.Acceleration == Vector2.Zero && phy.Velocity == Vector2.Zero)
                return;
                
            phy.RotationRadians += phy.AngularVelocity * deltaTime;
            phy.Velocity += phy.Acceleration;
            phy.Velocity *= 1f - phy.Drag;
            phy.AngularVelocity *= 1f - phy.Drag;

            phy.Velocity += phy.Mass * new Vector2(0,0.2f) * deltaTime;
            phy.Velocity = phy.Velocity.ClampMagnitude(SpeedLimit);

            phy.LastPosition = phy.Position;
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

            var shpEntity = PixelWorld.GetAttachedShapeEntity(in ntt);
            MovedEntitiesThisFrame.Push(shpEntity);
            var rect = shpEntity.Rect;
            rect.X = (int)phy.Position.X - shpEntity.Rect.Width / 2;
            rect.Y = (int)phy.Position.Y - shpEntity.Rect.Height / 2;
            shpEntity.Rect = rect;

            ref var vwp = ref ntt.Get<ViewportComponent>();
            vwp.Viewport.X = (int)phy.Position.X - vwp.ViewDistance / 2;
            vwp.Viewport.Y = (int)phy.Position.Y - vwp.ViewDistance / 2;

            // FConsole.WriteLine($"Speed: {phy.Velocity.Length()} - {phy.Velocity.Length() / 1000000 / 16.6 * 1000 * 60 * 60}kph");
        }
        protected override void PostUpdate()
        {            
            while(MovedEntitiesThisFrame.TryPop(out var ntt))
                Game.Tree.Move(ntt);
        }
    }
}