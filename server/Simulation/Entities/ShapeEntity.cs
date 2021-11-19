using System.Numerics;
using iogame.Net.Packets;
using iogame.Simulation.Components;
using iogame.Simulation.Managers;
using iogame.Util;

namespace iogame.Simulation.Entities
{

    public class ShapeEntity
    {
        public ECS.Entity Entity;
        public ShapeEntity Owner;
        public ushort VIEW_DISTANCE = 300;
        public int EntityId => Entity.EntityId;
        public ref PositionComponent PositionComponent => ref Entity.Get<PositionComponent>();
        public ref VelocityComponent VelocityComponent => ref Entity.Get<VelocityComponent>();
        public ref PhysicsComponent PhysicsComponent => ref Entity.Get<PhysicsComponent>();
        public ref HealthComponent HealthComponent => ref Entity.Get<HealthComponent>();
        public ref ShapeComponent ShapeComponent => ref Entity.Get<ShapeComponent>();
        public ref SpeedComponent SpeedComponent => ref Entity.Get<SpeedComponent>();
        public Screen Viewport;
        public uint LastShot;
        public float FireDir;

        public ShapeEntity()
        {
            Viewport = new PassiveScreen(this);
        }

        public void GetHitBy(ShapeEntity other)
        {
            
        }

        internal void Attack()
        {
            if (LastShot + 10 > Game.CurrentTick)
                return;

            LastShot = Game.CurrentTick;
            var dx = (float)Math.Cos(FireDir);
            var dy = (float)Math.Sin(FireDir);

            var bulletX = -dx + PositionComponent.Position.X;
            var bulletY = -dy + PositionComponent.Position.Y;
            var bullet = SpawnManager.Spawn<Bullet>(new Vector2(bulletX, bulletY));

            ref var pos = ref bullet.PositionComponent;
            ref var vel = ref bullet.Entity.Add<VelocityComponent>();
            ref var spd = ref bullet.Entity.Add<SpeedComponent>();
            ref var shp = ref bullet.Entity.Add<ShapeComponent>();
            ref var hlt = ref bullet.Entity.Add<HealthComponent>();
            ref var phy = ref bullet.Entity.Add<PhysicsComponent>();
            ref var ltc = ref bullet.Entity.Add<LifeTimeComponent>();
            ref var inp = ref bullet.Entity.Add<InputComponent>();
            ref var dmg = ref bullet.Entity.Add<DamageComponent>();

            spd.Speed = 50;
            shp.Sides = 0;
            shp.Size = 5;
            hlt.Health = 20;
            hlt.MaxHealth = 20;
            hlt.HealthRegenFactor = 0;
            phy.Mass = (float)Math.Pow(ShapeComponent.Size, 3);
            phy.Drag = 0;
            phy.Elasticity = 0;
            ltc.LifeTimeSeconds = 15;
            dmg.Damage = 1;

            var dist = PositionComponent.Position - pos.Position;
            var pen_depth = ShapeComponent.Radius + shp.Radius - dist.Magnitude();
            var pen_res = dist.Unit() * pen_depth * 1.125f;
            pos.Position += pen_res;
            inp.MovementAxis = new Vector2(dx, dy);
            vel.Acceleration = inp.MovementAxis * spd.Speed;

            bullet.Entity.Add(ref vel);
            bullet.Entity.Add(ref shp);
            bullet.Entity.Add(ref hlt);
            bullet.Entity.Add(ref phy);
            bullet.Entity.Add(ref ltc);
            bullet.Entity.Add(ref spd);

            bullet.Owner  = this;

            Viewport.Add(bullet, true);
        }

        internal void MoveFor(ShapeEntity owner) => (owner as Player)?.Send(MovementPacket.Create(EntityId, PositionComponent.Position, VelocityComponent.Velocity));
        internal bool IntersectsWith(ShapeEntity b) => ShapeComponent.Radius + b.ShapeComponent.Radius >= (b.PositionComponent.Position - PositionComponent.Position).Magnitude();
        public bool CanSee(ShapeEntity entity) => Vector2.Distance(PositionComponent.Position, entity.PositionComponent.Position) < VIEW_DISTANCE;
        internal void SpawnTo(ShapeEntity owner)
        {
            if (owner is not Player player)
                return;

            if (this is Player || this is Bullet || this is Boid)
                player.Send(SpawnPacket.Create(this));
            else
                player.Send(ResourceSpawnPacket.Create(this));
        }
        // Sends the 'Alive = False' Status packet to make the client remove the entity
        internal void DespawnFor(ShapeEntity owner) => (owner as Player)?.Send(StatusPacket.Create(EntityId, 0, StatusType.Alive));
    }
}