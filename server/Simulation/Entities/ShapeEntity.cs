using System.Numerics;
using System.Runtime.CompilerServices;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Entities
{

    public class ShapeEntity
    {
        public ECS.Entity Entity;
        public const int VIEW_DISTANCE = 4000;
        public int EntityId => Entity.EntityId;
        public ref PositionComponent PositionComponent => ref Entity.Get<PositionComponent>();
        public ref VelocityComponent VelocityComponent => ref Entity.Get<VelocityComponent>();
        public ref PhysicsComponent PhysicsComponent => ref Entity.Get<PhysicsComponent>();
        public ref HealthComponent HealthComponent => ref Entity.Get<HealthComponent>();
        public ref ShapeComponent ShapeComponent => ref Entity.Get<ShapeComponent>();
        public ref SpeedComponent SpeedComponent => ref Entity.Get<SpeedComponent>();
        public float BodyDamage;
        public Screen Viewport;

        public ShapeEntity()
        {
            Viewport = new(this);
            BodyDamage = 1;
        }

        public void GetHitBy(ShapeEntity other)
        {
            HealthComponent.Health -= other.BodyDamage;
            if (HealthComponent.Health < 0)
                HealthComponent.Health = 0;

            other.HealthComponent.Health -= BodyDamage;
            if (other.HealthComponent.Health < 0)
                other.HealthComponent.Health = 0;

            Viewport.Send(StatusPacket.Create(EntityId, (uint)HealthComponent.Health, StatusType.Health), true);
            other.Viewport.Send(StatusPacket.Create(other.EntityId, (uint)other.HealthComponent.Health, StatusType.Health), true);
        }
        internal void MoveFor(ShapeEntity owner) => (owner as Player)?.Send(MovementPacket.Create(EntityId, PositionComponent.Position, VelocityComponent.Movement));
        internal bool IntersectsWith(ShapeEntity b) => ShapeComponent.Radius + b.ShapeComponent.Radius >= (b.PositionComponent.Position - PositionComponent.Position).Magnitude();
        public bool CanSee(ShapeEntity entity) => Vector2.Distance(PositionComponent.Position, entity.PositionComponent.Position) < VIEW_DISTANCE;
        internal void SpawnTo(ShapeEntity owner)
        {
            if (owner is not Player player)
                return;
            
            if (this is Player || this is Bullet)
                player.Send(SpawnPacket.Create(this));
            else
                player.Send(ResourceSpawnPacket.Create(this));
        }
        // Sends the 'Alive = False' Status packet to make the client remove the entity
        internal void DespawnFor(ShapeEntity owner) => (owner as Player)?.Send(StatusPacket.Create(EntityId, 0, StatusType.Alive));
    }
}