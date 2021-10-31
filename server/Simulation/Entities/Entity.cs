using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Net.Packets;
using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{

    public class Entity
    {
        public const int VIEW_DISTANCE = 4000;
        public uint UniqueId;
        public PositionComponent PositionComponent;
        public VelocityComponent VelocityComponent;
        public PhysicsComponent PhysicsComponent;
        public HealthComponent HealthComponent;
        public ShapeComponent ShapeComponent;

        public float BodyDamage;
        public Screen Viewport;

        public Entity()
        {
            Viewport = new(this);
            BodyDamage = 1;
        }

        public void GetHitBy(Entity other)
        {
            HealthComponent.Health -= other.BodyDamage;
            if (HealthComponent.Health < 0)
                HealthComponent.Health = 0;

            other.HealthComponent.Health -= BodyDamage;
            if (other.HealthComponent.Health < 0)
                other.HealthComponent.Health = 0;

            Viewport.Send(StatusPacket.Create(UniqueId, (uint)HealthComponent.Health, StatusType.Health), true);
            other.Viewport.Send(StatusPacket.Create(other.UniqueId, (uint)other.HealthComponent.Health, StatusType.Health), true);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal bool IntersectsWith(Entity b) => ShapeComponent.Radius + b.ShapeComponent.Radius >= (b.PositionComponent.Position - PositionComponent.Position).Magnitude();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool CanSee(Entity entity) => Vector2.Distance(PositionComponent.Position, entity.PositionComponent.Position) < VIEW_DISTANCE;
    }
}