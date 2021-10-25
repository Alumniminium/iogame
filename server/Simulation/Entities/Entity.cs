using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Components;

namespace iogame.Simulation.Entities
{

    public unsafe class Entity
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
        }

        internal bool CheckCollision(Entity b) => ShapeComponent.Radius + b.ShapeComponent.Radius >= (b.PositionComponent.Position - PositionComponent.Position).Magnitude();
        public bool CanSee(Entity entity) => Vector2.Distance(PositionComponent.Position, entity.PositionComponent.Position) < VIEW_DISTANCE;
    }
}