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
        public uint LastShot;
        public float FireDir;

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

        internal void Attack()
        {
            if (LastShot + 1 > Game.CurrentTick)
                return;

            LastShot = Game.CurrentTick;
            var speed = 1000;
            var dx = (float)Math.Cos(FireDir);
            var dy = (float)Math.Sin(FireDir);

            var bulletX = -dx + PositionComponent.Position.X;
            var bulletY = -dy + PositionComponent.Position.Y;
            var bullet = SpawnManager.Spawn<Bullet>(new Vector2(bulletX, bulletY));

            ref var pos = ref bullet.PositionComponent;
            ref var vel = ref ComponentList<VelocityComponent>.AddFor(bullet.Entity.EntityId);
            ref var spd = ref ComponentList<SpeedComponent>.AddFor(bullet.Entity.EntityId);
            ref var shp = ref ComponentList<ShapeComponent>.AddFor(bullet.Entity.EntityId);
            ref var hlt = ref ComponentList<HealthComponent>.AddFor(bullet.Entity.EntityId);
            ref var phy = ref ComponentList<PhysicsComponent>.AddFor(bullet.Entity.EntityId);
            ref var ltc = ref bullet.LifeTimeComponent;

            spd.Speed = 1000;
            shp.Sides = 0;
            shp.Size = 25;
            hlt.Health = 20;
            hlt.MaxHealth = 20;
            hlt.HealthRegenFactor = 0;
            phy.Mass = (float)Math.Pow(ShapeComponent.Size, 3);
            phy.Drag = 0;
            phy.Elasticity = 0;
            ltc.LifeTimeSeconds = 5;

            var dist = PositionComponent.Position - pos.Position;
            var pen_depth = ShapeComponent.Radius + shp.Radius - dist.Magnitude();
            var pen_res = dist.Unit() * pen_depth * 1.125f;
            pos.Position += pen_res;
            vel.Force = new Vector2(dx * speed, dy * speed);

            bullet.Entity.Add(ref vel);
            bullet.Entity.Add(ref shp);
            bullet.Entity.Add(ref hlt);
            bullet.Entity.Add(ref phy);
            bullet.Entity.Add(ref ltc);
            bullet.Entity.Add(ref spd);

            bullet.SetOwner(this);

            Viewport.Add(bullet, true);
        }

        internal void MoveFor(ShapeEntity owner) => (owner as Player)?.Send(MovementPacket.Create(EntityId, PositionComponent.Position, VelocityComponent.Force));
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